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
     class line
    {
        public string a { get; set; }
        public int sum { get; set; }
    }
  unsafe  class Program
    {
     static   byte*[] pArray = new byte*[1000000];


        unsafe static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

           int[] ans = new int[100];
           int[][] arr = new int[80][];


           long[] aArray = new long[1000000];
           int[] mulArray = new int[1000000];

            var bytes = File.ReadAllBytes("test.csv");
            int start0 = 0;
            int start1 = 0;


            fixed (byte* p = bytes)
            fixed (long* paArray = aArray)
            fixed (int* pmulArray = mulArray)
            {
                LoadToLine(p);
                Vector128<int> Shift = Sse2.SetVector128(0, 8, 16, 24);
                //  Parallel.For(0, 8, j =>
                //  {
                //      int last = 125000 * j + 125000;
                Vector128<byte> offset = Sse2.SetAllVector128((byte)48);
                Vector128<byte> dot = Sse2.SetAllVector128((byte)46);
                byte insert = 255;
                // for (int i = 0; i < 1000000; i++)
                // {
                Parallel.For(0, 1000, j =>
              {
              byte* tp = stackalloc byte[16];

              int last = 1000 * j + 1000;
                  for (int i = 1000 * j; i < last; i++)
                  {
                      byte* tmp = tp;

                      byte* lp = pArray[i];
                      int n = ((int)*lp) + (((int)*(lp + 1)) << 8) + (((int)*(lp + 2)) << 16) + (((int)*(lp + 3)) << 24);
                      aArray[i] = (((long)*(lp + 4)) << 32) + n;
                      var cp = lp + 12;
                      var str = Sse2.LoadVector128(cp);
                      var nums = Sse2.Subtract(str, offset);

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

                      str = Sse2.Subtract(str, offset);
                      Sse2.Store(tmp, str);
                      float n0 = 0;
                      float n1 = 0;
                      if (dot0 == 1)
                      {
                          n0 += *tmp + 0.1f * *(tmp + 2) + 0.01f * *(tmp + 3);
                          tmp += 5;
                      }
                      else
                      {
                          n0 += *tmp * 10 + *(tmp + 1) + 0.1f * *(tmp + 3) + 0.01f * *(tmp + 4);
                          tmp += 6;
                      }

                      if (5 == dot1 - dot0)
                      {
                          n1 += *tmp + 0.1f * *(tmp + 2) + 0.01f * *(tmp + 3);
                      }
                      else
                      {
                          n1 += *tmp * 10 + *(tmp + 1) + 0.1f * *(tmp + 3) + 0.01f * *(tmp + 4);
                      }

                      mulArray[i] = (int)(n0 * n1 + 0.0001f);
                      //                   Sse2.int
                  }
              });




                List<long> alist = new List<long>();
                List<int> NoList = new List<int>();
                for (int i = 0; i < 1000000; i++)
                {
                    bool isOrigin = true;
                    for (int k = 0; k < alist.Count; k++)
                    {
                        if (alist[k] == aArray[i])
                        {
                            isOrigin = false;
                        }
                    }
                    if (isOrigin)
                    {
                        alist.Add(aArray[i]);
                        NoList.Add(i);
                    }
                    if (alist.Count >= 100)
                        break;
                }
                //});
                long[] alistArray = alist.ToArray();
                int[] nolistArray = NoList.ToArray();
                
                //    for (int c = 0; c < 1000; c++)
                Parallel.For(0, 80, c =>
                  {
                      arr[c] = new int[104];
                      int last = c * 12500 + 12500;
                      for (int i = c * 12500; i < last; i++)
                          //    for (int i = 0; i < 1000; i++)
                          for (int k = 0; k < 100; k++)
                          {
                              if (aArray[i] == alistArray[k])
                              {
                                  arr[c][k] += mulArray[i];
                                  break;
                              }
                          }
                  });


                Vector256<int>[] addsum = new Vector256<int>[13];
                for (int i = 0; i < 80; i++)
                {
                    fixed (int* parr = arr[i])
                    {
                        for (int j = 0; j < 13; j++)
                        {
                            var addtmp = Avx.LoadVector256(parr + 8 * j);
                            addsum[j] = Avx2.Add(addsum[j], addtmp);
                        }
                    }
                }

                 
                int* addsumArray =stackalloc int[104];
                    for (int i = 0; i < 13; i++)
                    {
                        Avx.Store(addsumArray + i * 8, addsum[i]);
                    }


                (string a, int sum)[] rtn = new(string, int)[100];
                for (int i = 0; i < 100; i++)
                {
                    var span = new Span<byte>(pArray[nolistArray[i]], 5);
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
            pArray[0]= p + 9;
            Vector128<byte> tmp = new Vector128<byte>();
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

                //bits = (bits & 0x0000ffff) + (bits >> 16 & 0x0000ffff);
            }
        }
    }
    
    struct Datas
    {
        long a;
        int sum;
    }
}