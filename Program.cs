using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
//Still in coding.
namespace ConsolePNGConv
{
    class Program
    {
        static Byte[] VFlip(Byte[,] Tile, int Tilenumber)
        {
            Byte[] TileVFlip = new Byte[32];
            for(int u=0; u<=31; u++)
            {
                if (u <= 3)
                    TileVFlip[u] = Tile[Tilenumber, 28 + u];
                else if (u <= 7)
                    TileVFlip[u] = Tile[Tilenumber, 24 + (u % 4)];
                else if (u <= 11)
                    TileVFlip[u] = Tile[Tilenumber, 20 + (u % 4)];
                else if (u <= 15)
                    TileVFlip[u] = Tile[Tilenumber, 16 + (u % 4)];
                else if (u <= 19)
                    TileVFlip[u] = Tile[Tilenumber, 12 + (u % 4)];
                else if (u <= 23)
                    TileVFlip[u] = Tile[Tilenumber, 8 + (u % 4)];
                else if (u <= 27)
                    TileVFlip[u] = Tile[Tilenumber, 4 + (u % 4)];
                else
                    TileVFlip[u] = Tile[Tilenumber, (u % 4)];
            }
            return TileVFlip;
        }
        static Byte[] HFlip(Byte[,] Tile, int Tilenumber)
        {
            Byte[] TileHFlip = new Byte[32];
            for (int u = 0; u <= 31; u++)
            {
                if ((u % 4) == 0)
                    TileHFlip[u] = (Byte)(((Tile[Tilenumber, u + 3] % 16) * 16) + (Tile[Tilenumber, u + 3]/16));
                else if ((u % 4) == 1)
                    TileHFlip[u] = (Byte)(((Tile[Tilenumber, u + 1] % 16) * 16) + (Tile[Tilenumber, u + 1]/16));
                else if ((u % 4) == 2)
                    TileHFlip[u] = (Byte)(((Tile[Tilenumber, u - 1] % 16) * 16) + (Tile[Tilenumber, u - 1]/16));
                else
                    TileHFlip[u] = (Byte)(((Tile[Tilenumber, u - 3] % 16) * 16) + (Tile[Tilenumber, u - 3]/16));
            }
            return TileHFlip;
        }
       static Byte[] Reverse(Byte[,] Tile, int Tilenumber)
        {
            Byte[,] TileReversing = new Byte[1,32];
            Byte[] TileReverse=new Byte[32];
            TileReverse = HFlip(Tile, Tilenumber);
            for (int k = 0; k < 32; k++)
                TileReversing[0, k] = TileReverse[k];
            TileReverse = VFlip(TileReversing, 0);
            return TileReverse;
        }
        static Byte[] Same(Byte[,] Tile, int Tilenumber)
        {
            Byte[] TileSame = new Byte[32];
            for (int u = 0; u <= 31; u++)
                TileSame[u] = Tile[Tilenumber, u];
                return TileSame;
        }
        static int Comparison(Byte[,]Tile, int Tilenumber, int Tilenumber2)
        {
            int Compared = 0, Use=0;
            Byte[] TileComp = Same(Tile, Tilenumber2);
            for (int k = 0; k < 32; k++) {
                if (Tile[Tilenumber, k] == TileComp[k])
                    Use += 1; //Every pixel must be the same.
                else
                    k = 32;
                    }
            if (Use == 32)
                Compared += 1;
            Use = 0;
            TileComp = HFlip(Tile, Tilenumber2);
            for (int k = 0; k < 32; k++)
            {
                if (Tile[Tilenumber, k] == TileComp[k])
                    Use += 1; //Every pixel must be HFlipped.
                else
                    k = 32;
            }
            if (Use == 32)
                Compared += 10;
            Use = 0;
            TileComp = VFlip(Tile, Tilenumber2);
            for (int k = 0; k < 32; k++)
            {
                if (Tile[Tilenumber, k] == TileComp[k])
                    Use += 1; //Every pixel must be VFlipped.
                else
                    k = 32;
            }
            if (Use == 32)
                Compared += 100;
            Use = 0;
            TileComp = Reverse(Tile, Tilenumber2);
            for (int k = 0; k < 32; k++)
            {
                if (Tile[Tilenumber, k] == TileComp[k])
                    Use += 1; //Every pixel must be Reversed.
                else
                    k = 32;
            }
            if (Use == 32)
                Compared += 1000;
            Use = 0;
            return Compared;
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Please enter the png path.");
            string Path = Console.ReadLine();
            int k = 0, lenght = 0, u = 0;
            List<int> Times = new List<int>();
            List<int> Connections = new List<int>();
            List<Color> Maxcolors = new List<Color>();
            Bitmap img = new Bitmap(Path);
            Console.WriteLine("Width: " + img.Width + ", Height: " + img.Height + ".");
            for (int i = 0; i < img.Height; i++)
            {
                for (int j = 0; j < img.Width; j++)
                {
                    Color pixel = img.GetPixel(j, i);
                    for (k = 0; k < lenght; k++)
                    {
                        u = 0;
                            if ((((pixel.R/8)&31)==((Maxcolors[k].R/8)&31))&& (((pixel.G / 8) & 31) == ((Maxcolors[k].G / 8) & 31)) && (((pixel.B / 8) & 31) == ((Maxcolors[k].B / 8) & 31)) && (pixel.A==Maxcolors[k].A)&&(k!=0))//Avoid colors that would be identical, but do not consider the first colour.
                            {
                                u = 1;
                                Times[k] += 1;
                                k = lenght;
                            }
                        }
                    if (u == 0)
                    {
                        lenght += 1;
                        Times.Add(0);
                        Maxcolors.Add(pixel);
                    }
                }
            }
            List<int> SortedTimes = Times.OrderByDescending(o => o).ToList();
            for (k = 0; k < lenght; k++)
            {
                for (u = 0; u < lenght; u++)
                {
                    if (SortedTimes[k] == Times[u])
                    {
                        Connections.Add(u);
                        u = lenght;
                    }
                }
            }
            if (Connections[0] != 0)
            {
                if (Connections.IndexOf(0) <= 15)
                    Connections[Connections.IndexOf(0)] = Connections[0];
                else
                    Connections[15] = Connections[0];
                Connections[0] = 0;
            }
            u = 0;
            List<Color> FinalColors = new List<Color>();
            for (k = 0; k < lenght; k++)
            {
                FinalColors.Add(Maxcolors[Connections[k]]);
            }
            Maxcolors.Clear();
            Connections.Clear();
            Times.Clear();
            SortedTimes.Clear();
            u = 0;
            if (FinalColors.Exists(x => x.A <= 128))
            {
                while (u == 0)
                {
                    if ((FinalColors.FindIndex(x => x.A <= 128)) != (FinalColors.FindLastIndex(x => x.A <= 128)))
                    {
                        FinalColors.RemoveAt(FinalColors.FindLastIndex(x => x.A <= 128));
                    }
                    else u = 1;
                }
            }
            if (FinalColors.Count > 16)
                FinalColors.RemoveRange(16, FinalColors.Count - 16);
            List<Byte> Realhex = new List<Byte>();
            List<UInt16> hexpal = new List<UInt16>();
            for(k=0;k<FinalColors.Count; k++)
            {
                hexpal.Add((UInt16)((((FinalColors[k].B>>3)&31)<<10) + (((FinalColors[k].G>>3)&31)<<5) + ((FinalColors[k].R>>3)&31)));
                byte[] intBytes= BitConverter.GetBytes(hexpal[k]);
                Realhex.AddRange(intBytes);
            }
            while (Realhex.Count < 32)
                Realhex.Add(0);
            byte[] PALette = Realhex.ToArray();
            File.WriteAllBytes("Testpalette.bin", PALette);
            Realhex = new List<Byte>();
            int height = img.Height;
            int width = img.Width;
            while ((width % 8) != 0)
                width += 1;
            while ((height % 8) != 0)
                height += 1;
            int tileheight = height / 8;
            int tilewidth = width / 8;
            byte rest;
            for (u = 0; u < tileheight; u++)
            {
                for(k=0; k < tilewidth; k++)
                {
                    for(int j=0; j<8; j++)
                    {
                        for(int i=0;i<4;i++)
                        {
                            rest = 0;
                            if (((u * 8) + j) < img.Height)
                            {
                                if ((8 * k) + (i * 2) < img.Width)
                                {
                                    Color pixel = img.GetPixel((8 * k) + (i * 2), (u * 8) + j);
                                    if (FinalColors.Exists(x => x == pixel))
                                    {
                                        rest = (Byte)(FinalColors.IndexOf(pixel));
                                    }
                                    else
                                    {
                                        int[] Similar = new int[FinalColors.Count];
                                        for (int d = 0; d < FinalColors.Count; d++)
                                        {
                                            Similar[d] = 0;
                                            if (FinalColors[d].A >= pixel.A)
                                                Similar[d] += 1000 * (FinalColors[d].A - pixel.A);
                                            else
                                                Similar[d] += 1000 * (pixel.A - FinalColors[d].A);
                                            if (FinalColors[d].B >= pixel.B)
                                                Similar[d] += FinalColors[d].B - pixel.B;
                                            else
                                                Similar[d] += pixel.B - FinalColors[d].B;
                                            if (FinalColors[d].G >= pixel.G)
                                                Similar[d] += FinalColors[d].G - pixel.G;
                                            else
                                                Similar[d] += pixel.G - FinalColors[d].G;
                                            if (FinalColors[d].R >= pixel.R)
                                                Similar[d] += FinalColors[d].R - pixel.R;
                                            else
                                                Similar[d] += pixel.R - FinalColors[d].R;
                                        }
                                        int e = 1000000000;
                                        int MostSimilar = 0;
                                        for (int d = 0; d < FinalColors.Count; d++)
                                        {
                                            if (Similar[d] <= e)
                                            {
                                                e = Similar[d];
                                                MostSimilar = d;
                                            }
                                        }
                                        rest=(Byte)(MostSimilar);
                                    }
                                }
                            }
                            if (((u * 8) + j) < img.Height)
                            {
                                if (((8 * k) + (i * 2) + 1) < img.Width)
                                {
                                    Color pixel = img.GetPixel((8 * k) + (i * 2) + 1, (u * 8) + j);
                                    if (FinalColors.Exists(x => x == pixel))
                                    {
                                        rest += (Byte)((FinalColors.IndexOf(pixel))*16);
                                    }
                                    else
                                    {
                                        int[] Similar = new int[FinalColors.Count];
                                        for (int d = 0; d < FinalColors.Count; d++)
                                        {
                                            Similar[d] = 0;
                                            if (FinalColors[d].A >= pixel.A)
                                                Similar[d] += 1000 * (FinalColors[d].A - pixel.A);
                                            else
                                                Similar[d] += 1000 * (pixel.A - FinalColors[d].A);
                                            if (FinalColors[d].B >= pixel.B)
                                                Similar[d] += FinalColors[d].B - pixel.B;
                                            else
                                                Similar[d] += pixel.B - FinalColors[d].B;
                                            if (FinalColors[d].G >= pixel.G)
                                                Similar[d] += FinalColors[d].G - pixel.G;
                                            else
                                                Similar[d] += pixel.G - FinalColors[d].G;
                                            if (FinalColors[d].R >= pixel.R)
                                                Similar[d] += FinalColors[d].R - pixel.R;
                                            else
                                                Similar[d] += pixel.R - FinalColors[d].R;
                                        }
                                        int e = 1000000000;
                                        int MostSimilar = 0;
                                        for (int d = 0; d < FinalColors.Count; d++)
                                        {
                                            if (Similar[d] <= e)
                                            {
                                                e = Similar[d];
                                                MostSimilar = d;
                                            }
                                        }
                                        rest+=(Byte)(MostSimilar * 16);
                                    }
                                }
                            }
                            Realhex.Add(rest);
                        }
                    }
                    
                }
            }
            FinalColors.Clear();
            for (k = 0; k <= 31; k++)
            {
                Realhex.Add(255);//Prepare the image for CCG compression.
            }
            Byte[] Image = Realhex.ToArray();
            Byte[,] Tile = new Byte[((Image.Count())/32), 32];
            for (u = 0; u < ((Image.Count()) / 32); u++)
            {
                for (int j = 0; j < 32; j++)
                {
                    Tile[u, j] = Image[(u * 32) + j];
                    //if(Tile[u,j]<=15)
                        //Console.Write("0"); Testing purpose.
                    //Console.Write("{0:X}|",Tile[u, j]);
                }
                //Console.WriteLine("");
            }
            List<List<int>>Reverse = new List<List<int>>();
            List<int> ReverseCorrespond = new List<int>();
            List<List<int>> Same = new List<List<int>>();
            List<int>SameCorrespond = new List<int>();
            List<List<int>> HFlip = new List<List<int>>();
            List<int> HFlipCorrespond = new List<int>();
            List<List<int>> VFlip = new List<List<int>>();
            List<int> VFlipCorrespond = new List<int>();
            for (u=0;u< ((Image.Count()) / 32); u++)
            {
                for(k= u+1; k<Image.Count()/32;k++)
                {
                    int Result = Comparison(Tile, u, k);
                        
                    if (Result >= 1000)
                    {
                        if (!ReverseCorrespond.Exists(x => x == k))
                        {
                            ReverseCorrespond.Add(k);
                            Reverse.Add(new List<int>());
                        }
                        Reverse[ReverseCorrespond.IndexOf(k)].Add(u);
                        Result -= 1000;
                    }
                    if(Result>=100)
                    {
                        if (!VFlipCorrespond.Exists(x => x == k))
                        {
                            VFlipCorrespond.Add(k);
                            VFlip.Add(new List<int>());
                        }
                        VFlip[VFlipCorrespond.IndexOf(k)].Add(u);
                        Result -= 100;
                    }
                    if (Result >= 10)
                    {
                        if (!HFlipCorrespond.Exists(x => x == k))
                        {
                            HFlipCorrespond.Add(k);
                            HFlip.Add(new List<int>());
                        }
                        HFlip[HFlipCorrespond.IndexOf(k)].Add(u);
                        Result -= 10;
                    }
                    if (Result == 1)
                    {
                        if (!SameCorrespond.Exists(x => x == k))
                        {
                            SameCorrespond.Add(k);
                            Same.Add(new List<int>());
                        }
                        Same[SameCorrespond.IndexOf(k)].Add(u);
                        Result -= 1;
                    }
                }
            }
            for(k=0; k < Image.Count() / 32; k++)
            {
                if (ReverseCorrespond.Exists(x => x == k))
                {
                    Console.Write("Tile {0:X} is reverse of ", k);
                    for (u = 0; u < Reverse[ReverseCorrespond.IndexOf(k)].Count(); u++)
                        Console.Write("|{0:X}|", Reverse[ReverseCorrespond.IndexOf(k)][u]);
                    Console.WriteLine("");
                }
            }
            for (k = 0; k < Image.Count() / 32; k++)
            {
                if (SameCorrespond.Exists(x => x == k))
                {
                    Console.Write("Tile {0:X} is the same of ",k);
                    for (u = 0; u < Same[SameCorrespond.IndexOf(k)].Count(); u++)
                        Console.Write("|{0:X}|", Same[SameCorrespond.IndexOf(k)][u]);
                    Console.WriteLine("");
                }
            }
            for (k = 0; k < Image.Count() / 32; k++)
            {
                if (HFlipCorrespond.Exists(x => x == k))
                {
                    Console.Write("Tile {0:X} is HFlipped of ",k);
                    for (u = 0; u < HFlip[HFlipCorrespond.IndexOf(k)].Count(); u++)
                        Console.Write("|{0:X}|", HFlip[HFlipCorrespond.IndexOf(k)][u]);
                    Console.WriteLine("");
                }
            }
            for (k = 0; k < Image.Count()/32; k++)
            {
                if (VFlipCorrespond.Exists(x => x == k))
                {
                    Console.Write("Tile {0:X} is VFlipped of ",k);
                    for (u = 0; u < VFlip[VFlipCorrespond.IndexOf(k)].Count(); u++)
                        Console.Write("|{0:X}|", VFlip[VFlipCorrespond.IndexOf(k)][u]);
                    Console.WriteLine("");
                }
            }
            File.WriteAllBytes("Testimage.bin", Image);
        }
    }
}
