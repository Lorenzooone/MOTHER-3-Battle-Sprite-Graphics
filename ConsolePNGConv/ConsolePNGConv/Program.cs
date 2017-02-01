using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
//Still in coding. 01: 80AECC5 9F64128 02: Phase 1: 80ADB55 9F61D08 Phase 2: 80ADA9D 9F61D10 
namespace ConsolePNGConv
{
    class Program
    {
        static List<Byte>SOBGen(Byte[,]Tile, int Tileheight, int Tilewidth, int Back)
        {
            List<Byte> SOB = new List<byte>();
            SOB.Add(0x73);
            SOB.Add(0x6F);
            SOB.Add(0x62);
            SOB.Add(0x20);
            if (Back == 1)
            {
                SOB.Add(2);
                SOB.Add(0);
                SOB.Add(2);
                SOB.Add(0);
            }
            else
            {
                SOB.Add(1);
                SOB.Add(0);
                SOB.Add(1);
                SOB.Add(0);
            }
            SOB.Add(0x7E);
            SOB.Add(0x73);
            SOB.Add(0x6F);
            SOB.Add(0x62);
            return SOB;
        }
        static Byte[,] TileOAM(Byte[,]Tile, int Tileheight, int Tilewidth, int Back, out int Modiheight, out int Modiwidth)
        {
            Modiwidth = Tilewidth;
            Modiheight = 0;
            int Limit = Tileheight, times = 0, more=0, mors=0;
            List<List<Byte>> TempTiles=new List<List<Byte>>();
            int o = Limit;
            for (int i = 0; i < Limit; i++)
            {
                Modiheight += 1;
                for (int k = 0; k < 8; k++)
                {
                    TempTiles.Add(new List<Byte>());
                    if (k >= Tilewidth)
                    {
                        for (int j = 0; j < 32; j++)
                            TempTiles[((i + more + (Limit * times)) * 8) + k].Add(0);
                    }
                    else { 
                    for (int j = 0; j < 32; j++)
                        TempTiles[((i+more) * 8) + k].Add(Tile[(i * Tilewidth) + k, j]);
                }
                    }
            }
            if (Tilewidth > 4)
            {
                while ((o % 4) != 0)
                {
                    Modiheight += 1;
                    for (int k = 0; k < 8; k++)
                    {
                        TempTiles.Add(new List<Byte>());
                        for (int j = 0; j < 32; j++)
                            TempTiles[(o * 8) + k].Add(0);
                    }
                    more++;
                    o++;
                }
            }
            o = Limit;
            times += 1;
           if (Tilewidth > 8)
            {
                for (int g = 0; g < (Tilewidth / 8); g++)
                {
                    for (int i = 0; i < Limit; i++)
                    {
                        Modiheight += 1;
                        for (int k = 0; k < 8; k++)
                        {
                            TempTiles.Add(new List<Byte>());
                            if ((8 * times + k) >= Tilewidth)
                            {
                                for (int j = 0; j < 32; j++)
                                    TempTiles[((i + more + (Limit * times)) * 8) + k].Add(0);
                            }
                            else {
                                for (int j = 0; j < 32; j++)
                                    TempTiles[((i + more + (Limit * times)) * 8) + k].Add(Tile[(i * Tilewidth) + 8 * times + k, j]);
                            }
                        }
                    }
                    if (Tilewidth > 4)
                    {
                        mors = more;
                        while ((o % 4) != 0)
                        {
                            Modiheight += 1;
                            for (int k = 0; k < 8; k++)
                            {
                                TempTiles.Add(new List<Byte>());
                                for (int j = 0; j < 32; j++)
                                    TempTiles[((o + mors + (Limit * times)) * 8) + k].Add(0);
                            }
                            more += 1;
                            o++;
                        }
                        o = Limit;
                    }
                    times += 1;
                }
            }
            for (mors = o; (mors % 4) != 0; mors++);
            int morn = (mors / 8) * 2;
            if (((Tilewidth % 8) == 5) || ((Tilewidth % 8) == 6))
            {
                for (int g = 0; g < 3; g++)
                {
                    for (int i = 0; i < morn; i++)
                {
                            for (int k = 0; k < 2; k++)
                                TempTiles[((i + more + (g*morn)+ (Limit * (times - 1))) * 8) + k + 6] = TempTiles[((i+(morn*3) +  more + (Limit * (times-1))) * 8) +(g*2)+ k];
                    }
                }
                Modiheight -= morn;
            }
            else if (((Tilewidth % 8) == 3) || ((Tilewidth % 8) == 4))
            {
                for (int i = 0; i < mors / 2; i++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        TempTiles[((i + more + (Limit * (times-1))) * 8) + k+4]=TempTiles[((i + (mors/2) + more + (Limit * (times-1))) * 8) + k];
                    }
                }
                Modiheight -= mors/2;
            }
            else if (((Tilewidth % 8) == 2)||((Tilewidth%8)==1))
            {
                for (int g = 0; g < 3; g++)
                {
                    for (int i = 0; i < morn; i++)
                    {
                        for (int k = 0; k < 2; k++)
                            TempTiles[((i + more + (Limit * (times - 1))) * 8) + (g*2) + k + 2] = TempTiles[((i + (morn * (g+1)) + more + (Limit * (times - 1))) * 8) + k];
                    }
                }
                Modiheight -= (morn*3);
            }
            Limit = TempTiles.Count();
            Byte[,] Newtiles=new Byte[Limit, 32];
            for (int i = 0; i < Limit; i++) {
                for (int k = 0; k < 32; k++)
                    Newtiles[i, k] = TempTiles[i][k];
            }
            if (Tilewidth > 4)
                Modiwidth = 8;
            else
                Modiwidth = 4;
            return Newtiles;
        }
        static Byte[] OAMGen(int Tilewidth, int Tileheight, int Back, out int OAMNum, out int BackOAMNum)
        {
            List<Byte> OAMList = new List<byte>();
            Byte Xcord = 0, Shape = 0, size = 0;
            int Limit = Tileheight, j=Tilewidth;
            if (Back == 1)
                Limit /= 2;
            OAMNum = 0;
            BackOAMNum = 0;
            int i = Limit;
            int Height = 272 - (Limit<<3);
            if (Height > 255)
                Height -= 255;
            OAMList.Add(01);
            OAMList.Add(0);
            OAMList.Add((Byte)Height);
            if ((i == 1)||(i == 3)) {
                if (j > 0)
                {
                    if ((j == 1)||(j == 3))
                    {
                        size = 0;
                        Shape = 0;
                        j -=1;
                    }
                    else if (j == 2)
                    {
                        size = 0;
                        Shape = 64;
                        j = 0;
                    }
                    else
                    {
                        size = 64;
                        Shape = 64;
                        j -= 4;
                    }
                    if (j > 4)
                    {

                    }
                }
                    }
            OAMList.Add(Shape);
            OAMList.Add(Xcord);
            OAMList.Add(size);
            Byte[] OAM=OAMList.ToArray();
            return OAM;
        }
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
        static Byte[,] RemoveExceedingTiles(byte[,]Tile,int Tilewidth, int Tileheight, out int Tilewdth, out int Tilehight)
        {
            int temp = 0, hu=0, hd=0, hl=0, hr=0, g=0;
            while (g == 0)
            {
                for (int u = (hu*Tilewidth); u < ((hu+1)*Tilewidth); u++)
                {
                    for (int k = 0; k < 32; k++)
                    {
                        if (Tile[u, k] == 0)
                            temp += 1;
                    }
                }
                if (temp == (32 * Tilewidth))
                {
                    hu += 1;
                }
                else
                    g = 1;
                temp = 0;
            }
            g = 0;
            while (g == 0)
            {
                for (int u = ((Tileheight-hd-1)*Tilewidth); u < ((Tileheight - hd)*Tilewidth); u++)
                {
                    for (int k = 0; k < 32; k++)
                    {
                        if (Tile[u, k] == 0)
                            temp += 1;
                    }
                }
                if (temp == (32 * Tilewidth))
                {
                    hd += 1;
                }
                else
                    g = 1;

                temp = 0;
            }
            g = 0;
            while (g == 0)
            {
                for (int u = hl; u <= (((Tileheight-1)*(Tilewidth))+hl); u=u+Tilewidth)
                {
                    for (int k = 0; k < 32; k++)
                    {
                        if (Tile[u, k] == 0)
                            temp += 1;
                    }
                }
                if (temp == (32 * Tileheight))
                {
                    hl += 1;
                }
                else
                    g = 1;
                temp = 0;
            }
            g = 0;
            while (g == 0)
            {
                for (int u = (Tilewidth-hr-1); u<(((Tileheight) * (Tilewidth)) +Tilewidth-hr-1); u = u + Tilewidth)
                {
                    for (int k = 0; k < 32; k++)
                    {
                        if (Tile[u, k] == 0)
                            temp += 1;
                    }
                }
                if (temp == (32 * (Tileheight)))
                {
                    hr += 1;
                }
                else
                    g = 1;
                temp = 0;
            }
            Tilehight = Tileheight - hu - hd;
            Tilewdth = Tilewidth - hl - hr;
            Byte[,] NewTile = new Byte[(Tilehight*Tilewdth),32];
            for(int f=0; f<(Tilehight); f++)
            {
                for (int d = 0; d < Tilewdth; d++)
                {
                    for (int k = 0; k < 32; k++)
                    {
                        NewTile[(d + (Tilewdth*f)), k] = Tile[hl +d+(Tilewidth*(f + hu)), k];
                    }
                }
            }
            return NewTile;
        }
        static Byte[,] SeparateBack(byte[,] Tile, int Tilewidth, int Tileheight, int Back, out int Tilehight, out int Tilehight2, out byte[,] TileBack)
        {
           if (Back == 1)
            {
                Tilehight = Tileheight / 2;
                Tilehight2 = Tileheight - Tilehight;
            }
            else {
                Tilehight = Tileheight;
                Tilehight2 = 0;
            }
            Byte[,] TileFront = new Byte[Tilewidth * Tilehight, 32];
            TileBack = new Byte[Tilewidth * Tilehight2, 32];
            for(int i=0; i < (Tilewidth * Tilehight); i++)
            {
                for (int j = 0; j < 32; j++)
                    TileFront[i, j] = Tile[i, j];
            }
            for(int i=0; i<(Tilehight2*Tilewidth);i++)
            {
                for (int j = 0; j < 32; j++)
                    TileBack[i, j] = Tile[i + (Tilewidth * Tilehight), j];
            }
            return TileFront;
        }
        static Byte[,] UniteTile(Byte[,]TileFront, Byte[,] TileBack, int Back, int Tilewidth, int Tileheight, int Tilewidth2, int Tileheight2, out int TilewidthUNITE, out int TileheightUNITE)
        {
            if (Tilewidth > Tilewidth2)
                TilewidthUNITE = Tilewidth;
            else
                TilewidthUNITE = Tilewidth2;
            TileheightUNITE = Tileheight + Tileheight2;
            Byte[,] Tile = new Byte[TilewidthUNITE * TileheightUNITE, 32];
            for (int i=0; i<Tileheight; i++)
            {
                for(int k=0; k<TilewidthUNITE; k++)
                {
                    if (k>= Tilewidth)
                    {
                        for(int j=0; j<32; j++)
                        Tile[(i * TilewidthUNITE) + k, j] = 0;
                    }
                    else
                        for (int j = 0; j < 32; j++)
                            Tile[(i * TilewidthUNITE) + k, j]= TileFront[(i * Tilewidth) + k, j];
              }
            }
            if (Back == 1)
            {
                for (int i = 0; i < Tileheight2; i++)
                {
                    for (int k = 0; k < TilewidthUNITE; k++)
                    {
                        if (k >= Tilewidth2)
                        {
                            for (int j = 0; j < 32; j++)
                                Tile[((i+Tileheight) * TilewidthUNITE) + k, j] = 0;
                        }
                        else
                            for (int j = 0; j < 32; j++)
                                Tile[((i + Tileheight) * TilewidthUNITE) + k, j] = TileBack[(i * Tilewidth2) + k, j];
                    }
                }
            }
            return Tile;
        }
            static void Main(string[] args)
        {
            Console.WriteLine("Please enter the png path.");
            string Path = Console.ReadLine();
            int Back = 2;
            while ((Back != 0) && (Back != 1))
            {
                Console.WriteLine("If the enemy has a back sprite, please make sure it starts at the middle of the image and enter 1. Otherwise enter 0.");
                Back = Convert.ToInt16(Console.ReadLine());
            }
            int k = 0, lenght = 0, u = 0;
            List<int> Times = new List<int>();
            List<int> Connections = new List<int>();
            List<Color> Maxcolors = new List<Color>();
            Bitmap img = new Bitmap(Path);
            int height = img.Height;
            int width = img.Width;
            Console.WriteLine("Width: " + img.Width + ", Height: " + img.Height + ".");
            for (int i = 0; i < img.Height; i++)
            {
                for (int j = 0; j < img.Width; j++)
                {
                    Color pixel = img.GetPixel(j, i);
                    for (k = 0; k < lenght; k++)
                    {
                        u = 0;
                        if ((((pixel.R / 8) & 31) == ((Maxcolors[k].R / 8) & 31)) && (((pixel.G / 8) & 31) == ((Maxcolors[k].G / 8) & 31)) && (((pixel.B / 8) & 31) == ((Maxcolors[k].B / 8) & 31)) && (pixel.A == Maxcolors[k].A) && (k != 0))//Avoid colors that would be identical, but do not consider the first colour.
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
            while ((width % 8) != 0)
                width += 1;
            while ((height % 8) != 0)
                height += 1;
            int tileheight = height / 8;
            int tilewidth = width / 8;
            if ((tileheight * tilewidth) > 1024)
            {
                Console.WriteLine("Error! Image is too big!");
                return;
            }
            if (FinalColors.Count > 16)
                FinalColors.RemoveRange(16, FinalColors.Count - 16);
            List<Byte> Realhex = new List<Byte>();
            List<UInt16> hexpal = new List<UInt16>();
            for (k = 0; k < FinalColors.Count; k++)
            {
                hexpal.Add((UInt16)((((FinalColors[k].B >> 3) & 31) << 10) + (((FinalColors[k].G >> 3) & 31) << 5) + ((FinalColors[k].R >> 3) & 31)));
                byte[] intBytes = BitConverter.GetBytes(hexpal[k]);
                Realhex.AddRange(intBytes);
            }
            while (Realhex.Count < 32)
                Realhex.Add(0);
            byte[] PALette = Realhex.ToArray();
            File.WriteAllBytes("Testpalette.bin", PALette);
            Realhex = new List<Byte>();
            byte rest;
            for (u = 0; u < tileheight; u++)
            {
                for (k = 0; k < tilewidth; k++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        for (int i = 0; i < 4; i++)
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
                                        rest = (Byte)(MostSimilar);
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
                                        rest += (Byte)((FinalColors.IndexOf(pixel)) * 16);
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
                                        rest += (Byte)(MostSimilar * 16);
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
            Byte[,] Tile = new Byte[((Image.Count()) / 32), 32];
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
            byte[,] NewTile = RemoveExceedingTiles(Tile, tilewidth, tileheight, out tilewidth, out tileheight);
            List<List<int>> Reverse = new List<List<int>>();
            List<int> ReverseCorrespond = new List<int>();
            List<List<int>> Same = new List<List<int>>();
            List<int> SameCorrespond = new List<int>();
            List<List<int>> HFlip = new List<List<int>>();
            List<int> HFlipCorrespond = new List<int>();
            List<List<int>> VFlip = new List<List<int>>();
            List<int> VFlipCorrespond = new List<int>();
            for (u = 0; u < (tileheight * tilewidth); u++)
            {
                for (k = u + 1; k < (tileheight * tilewidth); k++)
                {
                    int Result = Comparison(NewTile, u, k);

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
                    if (Result >= 100)
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
            Byte[,] Tileback, OAMTile;
            int tilewidth1 = tilewidth;
            int tileheight1 = 0;
            int tilewidth2 = tilewidth;
            int tileheight2 = 0;
            int Tiletemph = tileheight;
            int Tiletempw = tilewidth;
            if (Back== 1)
            {
                NewTile = SeparateBack(NewTile, tilewidth, tileheight, Back, out tileheight1, out tileheight2, out Tileback);
                NewTile = RemoveExceedingTiles(NewTile, tilewidth, tileheight1, out tilewidth1, out tileheight1);
                Tileback = RemoveExceedingTiles(Tileback, tilewidth2, tileheight2, out tilewidth2, out tileheight2);
                Tileback = TileOAM(Tileback, tileheight2, tilewidth2, Back, out tileheight2, out tilewidth2);
                OAMTile = TileOAM(NewTile, tileheight1, tilewidth1, Back, out tileheight1, out tilewidth1);
                OAMTile = UniteTile(OAMTile, Tileback, Back, tilewidth1, tileheight1, tilewidth2, tileheight2, out tilewidth, out tileheight);
            }
            else
            OAMTile =TileOAM(NewTile, tileheight, tilewidth, Back, out tileheight, out tilewidth);
            /*
            int j, p;
            for (u = 0; u < (tileheight * tilewidth); u++)
            {
                for (k = u + 1; k < (tileheight * tilewidth); k++)
                {
                    int f = k;
                    for (p = 0; p < 8; p++)
                    {
                        for (j = 0; j < 8; j++)
                        {
                            if (ReverseCorrespond.Exists(x => x == f+ j))
                            {
                                if (Reverse[ReverseCorrespond.IndexOf(f + j)].Exists(x => x == u + j))
                                {
                                    f += 1;
                                }
                            }
                            else {
                                j += 19;
                                p += 19;
                            }
                        }
                        f = k + (tilewidth * p);

                    }
                }
            }*/
            Image = new Byte[((tileheight * tilewidth)+1) * 32];
            for (u = 0; u < tileheight * tilewidth; u++)
            {
                for (k = 0; k < 32; k++)
                    Image[(u * 32) + k] = OAMTile[u, k];
            }
            for (k = 0; k <= 31; k++)
            {
                Image[((tileheight*tilewidth)*32)+k]=255;//Prepare the image for CCG compression.
            }
            File.WriteAllBytes("Testimage.bin", Image);
            return;
        }
    }
}
