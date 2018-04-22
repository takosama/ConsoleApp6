using System;
using System.Threading.Tasks;
using System.Threading ;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;

namespace app
{
    unsafe class Program
    {

        static byte*[] pArray = new byte*[1000000];

        unsafe static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int[] ans = new int[100];
            long[] aArray = new long[1000000];
            int[] mulArray = new int[1000000];
            int[][] arr = new int[800][];


            Parallel.For(0, 800, i =>
            {
                arr[i] = new int[104];
            });

            var bytes = File.ReadAllBytes("test.csv");


            fixed (byte* p = bytes)
            fixed (long* paArray = aArray)
            fixed (int* pmulArray = mulArray)
            {
                LoadToLine(p);
                Vector128<byte> offset = Sse2.SetAllVector128((byte)48);
                Vector128<byte> dot = Sse2.SetAllVector128((byte)46);
                byte insert = 255;
                Parallel.For(0, 100, j =>
              {
                  byte* tp = stackalloc byte[16];
                  int last = 10000 * j + 10000;
                  for (int i = 10000 * j; i < last; i++)
                  {
                      var tmp = tp;

                      byte* lp = pArray[i];
                      int n = ((int)*lp) + (((int)*(lp + 1)) << 8) + (((int)*(lp + 2)) << 16) + (((int)*(lp + 3)) << 24);

                      aArray[i] = (((long)*(lp + 4)) << 32) + n;
                      var cp = lp + 12;
                      var str = Sse2.LoadVector128(cp);
                      var num = Sse2.Subtract(str, offset);
                      Sse2.Store(tmp, num);
                      var t = Sse2.CompareEqual(str, dot);
                      n = Sse2.MoveMask(t);
                      n = (n & (-n)) - 1;
                      n = (n & 0x55555555) + (n >> 1 & 0x55555555);
                      n = (n & 0x33333333) + (n >> 2 & 0x33333333);
                      n = (n & 0x0f0f0f0f) + (n >> 4 & 0x0f0f0f0f);
                      n = (n & 0x00ff00ff) + (n >> 8 & 0x00ff00ff);


                      int dot0 = n;
                      str = Sse41.Insert(str, insert, (byte)n);
                      t = Sse2.CompareEqual(str, dot);
                      n = Sse2.MoveMask(t);
                      n = (n & (-n)) - 1;
                      n = (n & 0x55555555) + (n >> 1 & 0x55555555);
                      n = (n & 0x33333333) + (n >> 2 & 0x33333333);
                      n = (n & 0x0f0f0f0f) + (n >> 4 & 0x0f0f0f0f);
                      n = (n & 0x00ff00ff) + (n >> 8 & 0x00ff00ff);

                      int dot1 = n;
                      double n0 = 0;
                      double n1 = 0;

                      if (dot0 == 2)
                      {
                          n0 = (1000 * *tmp + 100 * *(tmp + 1) + 10 * *(tmp + 3) + *(tmp + 4)) * 0.01;
                          tmp += 6;
                      }
                      else if (dot0 == 1)
                      {
                          n0 = (100 * *tmp + 10 * *(tmp + 2) + *(tmp + 3)) * 0.01;
                          tmp += 5;
                      }
                      else
                      {
                          n0 = 100;
                          tmp += 7;
                      }

                      if (dot1 - dot0 == 6)
                          n1 = (1000 * *tmp + 100 * *(tmp + 1) + 10 * *(tmp + 3) + *(tmp + 4)) * 0.01;
                      else if (dot1 - dot0 == 5)

                          n1 = (100 * *tmp + (10 * *(tmp + 2) + *(tmp + 3))) * 0.01;
                      else
                          n1 = 100;

                      mulArray[i] = (int)((n0 * n1) + 0.00000001);
                  }
              });

                long[] alist = new long[100];
                int[] NoList = new int[100];
                int cnt = -1;
                for (int i = 0; i < 1000000; i++)
                {
                    bool isOrigin = true;
                    for (int k = 0; k < alist.Length; k++)
                        if (alist[k] == aArray[i])
                            isOrigin = false;
                    if (isOrigin)
                    {
                        alist[++cnt] = aArray[i];
                        NoList[cnt] = i;
                    }
                    if (cnt == 99)
                        break;
                }


                Parallel.For(0, 800, c =>
                  {
                      int last = c * 1250 + 1250;
                      for (int i = c * 1250; i < last; i++)
                          for (int k = 0; k < 100; k++)
                              if (aArray[i] == alist[k])
                              {
                                  arr[c][k] += mulArray[i];
                                  break;
                              }
                  });

                Vector256<int>[] addsum = new Vector256<int>[13];
                for (int i = 0; i < 800; i++)
                {
                    fixed (int* parr = arr[i])
                        for (int j = 0; j < 13; j++)
                        {
                            var addtmp = Avx.LoadVector256(parr + 8 * j);
                            addsum[j] = Avx2.Add(addsum[j], addtmp);
                        }
                }


                int* addsumArray = stackalloc int[104];
                for (int i = 0; i < 13; i++)
                    Avx.Store(addsumArray + i * 8, addsum[i]);

                (string a, int sum)[] rtn = new(string, int)[100];
                for (int i = 0; i < 100; i++)
                {
                    var span = new Span<byte>(pArray[NoList[i]], 5);
                    var byts = span.ToArray();
                    var s = new string(byts.Select(x => (char)x).ToArray());
                    rtn[i] = (s, addsumArray[i]);
                }

                var jsonbytes = Utf8Json.JsonSerializer.SerializeUnsafe(rtn);
                using (var fs = File.OpenWrite("output,json"))
                {
                    fs.Write(jsonbytes.ToArray(), 0, jsonbytes.Count);
                }
            }




            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
        }

        private static unsafe void LoadToLine(byte* p)
        {
            byte* pp = p + 27;
            pArray[0] = p + 9;
            Vector128<byte> cmp = Sse2.SetAllVector128((byte)10);
            for (int i = 1; i < 1000000; i++)
            {
                //終端検索
                var backstr = Sse2.LoadVector128(pp);
                var t = Sse2.CompareEqual(backstr, cmp);
                int n = Sse2.MoveMask(t);
                n = (n & (-n)) - 1;
                n = (n & 0x55555555) + (n >> 1 & 0x55555555);
                n = (n & 0x33333333) + (n >> 2 & 0x33333333);
                n = (n & 0x0f0f0f0f) + (n >> 4 & 0x0f0f0f0f);
                n = (n & 0x00ff00ff) + (n >> 8 & 0x00ff00ff);


                pArray[i] = pp + n + 1;

                pp += 19 + n;

            }
        }
    }

}