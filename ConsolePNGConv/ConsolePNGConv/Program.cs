using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
//Still in coding. 01: 80AECC5 9F64128 02: Phase 1: 80ADB55 9F61D08 Phase 2: 80ADA9D 9F61D10
namespace GBA
{
    class LZ77
    {
        public static int Decompress(byte[] data, int address, out byte[] output)
        {
            output = null;
            int start = address;

            if (data[address++] != 0x10) return -1; // Check for LZ77 signature

            // Read the block length
            int length = data[address++];
            length += (data[address++] << 8);
            length += (data[address++] << 16);
            output = new byte[length];

            int bPos = 0;
            while (bPos < length)
            {
                byte ch = data[address++];
                for (int i = 0; i < 8; i++)
                {
                    switch ((ch >> (7 - i)) & 1)
                    {
                        case 0:

                            // Direct copy
                            if (bPos >= length) break;
                            output[bPos++] = data[address++];
                            break;

                        case 1:

                            // Compression magic
                            int t = (data[address++] << 8);
                            t += data[address++];
                            int n = ((t >> 12) & 0xF) + 3;    // Number of bytes to copy
                            int o = (t & 0xFFF);

                            // Copy n bytes from bPos-o to the output
                            for (int j = 0; j < n; j++)
                            {
                                if (bPos >= length) break;
                                output[bPos] = output[bPos - o - 1];
                                bPos++;
                            }

                            break;

                        default:
                            break;
                    }
                }
            }

            return address - start;
        }

        public static byte[] Compress(byte[] data)
        {
            return Compress(data, 0, data.Length);
        }

        public static byte[] Compress(byte[] data, int address, int length)
        {
            int start = address;

            List<byte> obuf = new List<byte>();
            List<byte> tbuf = new List<byte>();
            int control = 0;

            // Let's start by encoding the signature and the length
            obuf.Add(0x10);
            obuf.Add((byte)(length & 0xFF));
            obuf.Add((byte)((length >> 8) & 0xFF));
            obuf.Add((byte)((length >> 16) & 0xFF));

            while ((address - start) < length)
            {
                tbuf.Clear();
                control = 0;
                for (int i = 0; i < 8; i++)
                {
                    bool found = false;

                    // First byte should be raw
                    if (address == start)
                    {
                        tbuf.Add(data[address++]);
                        found = true;
                    }
                    else if ((address - start) >= length)
                    {
                        break;
                    }
                    else
                    {
                        // We're looking for the longest possible string
                        // The farthest possible distance from the current address is 0x1000
                        int max_length = -1;
                        int max_distance = -1;

                        for (int k = 1; k <= 0x1000; k++)
                        {
                            if ((address - k) < start) break;

                            int l = 0;
                            for (; l < 18; l++)
                            {
                                if (((address - start + l) >= length) ||
                                    (data[address - k + l] != data[address + l]))
                                {
                                    if (l > max_length)
                                    {
                                        max_length = l;
                                        max_distance = k;
                                    }
                                    break;
                                }
                            }

                            // Corner case: we matched all 18 bytes. This is
                            // the maximum length, so don't bother continuing
                            if (l == 18)
                            {
                                max_length = 18;
                                max_distance = k;
                                break;
                            }
                        }

                        if (max_length >= 3)
                        {
                            address += max_length;

                            // We hit a match, so add it to the output
                            int t = (max_distance - 1) & 0xFFF;
                            t |= (((max_length - 3) & 0xF) << 12);
                            tbuf.Add((byte)((t >> 8) & 0xFF));
                            tbuf.Add((byte)(t & 0xFF));

                            // Set the control bit
                            control |= (1 << (7 - i));

                            found = true;
                        }
                    }

                    if (!found)
                    {
                        // If we didn't find any strings, copy the byte to the output
                        tbuf.Add(data[address++]);
                    }
                }

                // Flush the temp buffer
                obuf.Add((byte)(control & 0xFF));
                obuf.AddRange(tbuf.ToArray());
            }
            while ((obuf.Count() % 4) != 0)
                obuf.Add(0);
            return obuf.ToArray();
        }
    }
}
namespace ConsolePNGConv
{
    class Program
    {
        static Byte[,]Finalize(Byte[,]Tile, ref Byte[] SOB, int Tilewidth)
        {
            int a, Shape, Size, Tilestart, XSize, YSize, Tilecount=0;
            Byte[,] Finalized = new Byte[Tile.Length/32,32];
            int OAMNum = SOB[(SOB[8]+(SOB[9]<<8)) + 2] + (SOB[(SOB[8] +( SOB[9] << 8)) + 3] << 8);
            a = (SOB[8] + (SOB[9] << 8)) + 5;
            while (OAMNum > 0)
            {
                Shape = SOB[a++];
                a++;
                Size = SOB[a++];
                if (Size % 2 != 0)
                    Size -= 1;
                Tilestart = SOB[a] + (SOB[a + 1] << 8);
                SOB[a] = (Byte)(Tilecount & 0xFF);
                SOB[a + 1] = (Byte)((Tilecount >> 8)&0x3);
                if (Shape == 0)
                {
                    if (Size == 0)
                        XSize = 1;
                    else if (Size == 0x40)
                        XSize = 2;
                    else if (Size == 0x80)
                        XSize = 4;
                    else
                        XSize = 8;
                    YSize = XSize;
                }
                else if (Shape == 0x40)
                {
                    if (Size == 0)
                    {
                        XSize = 2;
                        YSize = 1;
                    }
                    else if (Size == 0x40)
                    {
                        XSize = 4;
                        YSize = 1;
                    }
                    else if (Size == 0x80)
                    {
                        XSize = 4;
                        YSize = 2;
                    }
                    else
                    {
                        XSize = 8;
                        YSize = 4;
                    }
                }
                else
                {
                    if (Size == 0)
                    {
                        XSize = 1;
                        YSize = 2;
                    }
                    else if (Size == 0x40)
                    {
                        XSize = 1;
                        YSize = 4;
                    }
                    else if (Size == 0x80)
                    {
                        XSize = 2;
                        YSize = 4;
                    }
                    else
                    {
                        XSize = 4;
                        YSize = 8;
                    }
                }
                for(int i=0; i < YSize; i++)
                {
                    for(int j=0; j<XSize; j++)
                    {
                        for (int k = 0; k < 32; k++)
                        {
                            Finalized[Tilecount, k] = Tile[Tilestart + (i * Tilewidth) + j, k];
                        }
                        Tilecount++;
                    }
                }
                a += 5;
                OAMNum -= 1;
            }
            if ((SOB[8] + (SOB[9] << 8)) != (SOB[10] + (SOB[11] << 8))){
                OAMNum = SOB[(SOB[10] + (SOB[11] << 8)) + 2] + (SOB[(SOB[10] + (SOB[11] << 8)) + 3] << 8);
                a = (SOB[10] + (SOB[11] << 8)) + 5;
                while (OAMNum > 0)
                {
                    Shape = SOB[a++];
                    a++;
                    Size = SOB[a++];
                    if (Size % 2 != 0)
                        Size -= 1;
                    Tilestart = SOB[a] + (SOB[a + 1] << 8);
                    SOB[a] = (Byte)(Tilecount & 0xFF);
                    SOB[a + 1] = (Byte)((Tilecount >> 8) & 0x3);
                    if (Shape == 0)
                    {
                        if (Size == 0)
                            XSize = 1;
                        else if (Size == 0x40)
                            XSize = 2;
                        else if (Size == 0x80)
                            XSize = 4;
                        else
                            XSize = 8;
                        YSize = XSize;
                    }
                    else if (Shape == 0x40)
                    {
                        if (Size == 0)
                        {
                            XSize = 2;
                            YSize = 1;
                        }
                        else if (Size == 0x40)
                        {
                            XSize = 4;
                            YSize = 1;
                        }
                        else if (Size == 0x80)
                        {
                            XSize = 4;
                            YSize = 2;
                        }
                        else
                        {
                            XSize = 8;
                            YSize = 4;
                        }
                    }
                    else
                    {
                        if (Size == 0)
                        {
                            XSize = 1;
                            YSize = 2;
                        }
                        else if (Size == 0x40)
                        {
                            XSize = 1;
                            YSize = 4;
                        }
                        else if (Size == 0x80)
                        {
                            XSize = 2;
                            YSize = 4;
                        }
                        else
                        {
                            XSize = 4;
                            YSize = 8;
                        }
                    }
                    for (int i = 0; i < YSize; i++)
                    {
                        for (int j = 0; j < XSize; j++)
                        {
                            for (int k = 0; k < 32; k++)
                            {
                                Finalized[Tilecount, k] = Tile[Tilestart + (i * Tilewidth) + j, k];
                            }
                            Tilecount++;
                        }
                    }
                    a += 5;
                    OAMNum -= 1;
                }
            }
                return Finalized;
        }
        static Byte[]SOBGen(Byte[]OAMFront,Byte[]OAMBack, int TileheightFront, int TilewidthFront, int Back)
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
                SOB.Add(0x10);
                SOB.Add(0);
                SOB.Add((Byte)(0x14 + OAMFront.Count()));
                SOB.Add(0);
                SOB.Add((Byte)(0x18 + OAMFront.Count() + OAMBack.Count()));
                SOB.Add(0);
                SOB.Add((Byte)(0x18 + OAMFront.Count() + OAMBack.Count()));
                SOB.Add(0);
            }
            else
            {
                SOB.Add(2);
                SOB.Add(0);
                SOB.Add(2);
                SOB.Add(0);
                SOB.Add(0x10);
                SOB.Add(0);
                SOB.Add(0x10);
                SOB.Add(0);
                SOB.Add((Byte)(0x14 + OAMFront.Count()));
                SOB.Add(0);
                SOB.Add((Byte)(0x14 + OAMFront.Count()));
                SOB.Add(0);
            }
            SOB.Add(0);
            SOB.Add(0);
            SOB.Add((Byte)((OAMFront.Count() / 8) & 0xFF));
            SOB.Add((Byte)(((OAMFront.Count() / 8)>>8) & 0xFF));
            SOB.AddRange(OAMFront);
            if (Back == 1)
            {
                SOB.Add(0);
                SOB.Add(0);
                SOB.Add((Byte)((OAMBack.Count() / 8) & 0xFF));
                SOB.Add((Byte)(((OAMBack.Count() / 8) >> 8) & 0xFF));
                for(int k=0; k<OAMBack.Count(); k++)
                {
                    if (k % 8 != 4)
                        SOB.Add(OAMBack[k]);
                    else
                    {
                        SOB.Add((Byte)((OAMBack[k] + (TilewidthFront * TileheightFront)) & 0xFF));
                        SOB.Add(((Byte)(OAMBack[k+1]+(((OAMBack[k] + (TilewidthFront * TileheightFront)) >> 8) & 0x3))));
                        k++;
                        if ((SOB[SOB.Count()-1]) > 3)
                        {
                            Console.WriteLine("Too many tiles in the end!");
                            Environment.Exit(01);
                        }
                    }
                }
            }
            SOB.Add(4);
            SOB.Add(0);
            SOB.Add(1);
            SOB.Add(0);
            SOB.Add(0);
            SOB.Add(0);
            SOB.Add(0);
            SOB.Add(0);
            SOB.Add(0x7E);
            SOB.Add(0x73);
            SOB.Add(0x6F);
            SOB.Add(0x62);
            return SOB.ToArray();
        }
        static Byte[,] TileOAM(Byte[,]Tile, int Tileheight, int Tilewidth, int Back, out int Modiheight, out int Modiwidth, out Byte[] OAM)
        {
            Modiwidth = Tilewidth;
            Modiheight = 0;
            int Limit = Tileheight, times = 0, more=0, mors=0;
            List<List<Byte>> TempTiles=new List<List<Byte>>();
            List<List<Byte>> TumpTiles=new List<List<Byte>>();
            int o = Limit;
            if (Tilewidth > 4)
                Modiwidth = 8;
            else if(Tilewidth>2)
                Modiwidth = 4;
            for (int i = 0; i < Limit; i++)
            {
                Modiheight += 1;
                for (int k = 0; k < Modiwidth; k++)
                {
                    TumpTiles.Add(new List<Byte>());
                    if (k >= Tilewidth)
                    {
                        for (int j = 0; j < 32; j++)
                            TumpTiles[(i * Modiwidth) + k].Add(0);
                    }
                    else { 
                    for (int j = 0; j < 32; j++)
                        TumpTiles[(i * Modiwidth) + k].Add(Tile[(i * Tilewidth) + k, j]);
                }
                    }
            }
			int Singularwidth, Singularheight, XAdd=0;
            List<int> X=new List<int>(), Y = new List<int>(), XSize = new List<int>(), YSize = new List<int>(), Tilestart=new List<int>();
            X.Add(0);
            Y.Add(0);
            XSize.Add(Modiwidth);
            YSize.Add(Tileheight);
            Tilestart.Add(0);
            TumpTiles =RemoveExceedTiles(TumpTiles, Modiwidth, Tileheight, out Singularwidth, out Singularheight, ref Modiheight, ref XSize, ref YSize, ref X, ref Y, ref XAdd);
            TumpTiles = FixTile(TumpTiles, Singularwidth, Tilewidth, ref Singularheight, ref Modiheight, ref XSize, ref YSize);
            int XTemp = X[X.Count() - 1] + XSize[XSize.Count() - 1];
            MoveTile(ref TumpTiles, Singularwidth, ref Singularheight, ref Modiheight, 0, Tilewidth, XAdd, ref XSize, ref YSize, ref X, ref Y, ref Tilestart);
			TempTiles.AddRange(TumpTiles);
            TumpTiles=new List<List<Byte>>();
            times += 1;
           if (Tilewidth > 8)
            {
                for (int g = 0; g < (Tilewidth / 8); g++)
                {
                    Tilestart.Add(Modiheight * 8);
                    mors =more;
                    for (int i = 0; i < Limit; i++)
                    {
                        Modiheight += 1;
                        for (int k = 0; k < 8; k++)
                        {
                            TumpTiles.Add(new List<Byte>());
                            if ((8 * times + k) >= Tilewidth)
                            {
                                for (int j = 0; j < 32; j++)
                                    TumpTiles[(i  * 8) + k].Add(0);
                            }
                            else {
                                for (int j = 0; j < 32; j++)
                                    TumpTiles[(i * 8) + k].Add(Tile[(i * Tilewidth) + 8 * times + k, j]);
                            }
                        }
                    }
                    X.Add(XTemp + XAdd);
                    Y.Add(0);
                    XSize.Add(8);
                    YSize.Add(Tileheight);
                    TumpTiles =RemoveExceedTiles(TumpTiles, 8, Tileheight, out Singularwidth, out Singularheight, ref Modiheight, ref XSize, ref YSize, ref X, ref Y, ref XAdd);
			        TumpTiles=FixTile(TumpTiles, Singularwidth, Tilewidth, ref Singularheight, ref Modiheight, ref XSize, ref YSize);
                    XTemp = X[X.Count() - 1] + XSize[XSize.Count() - 1];
                    MoveTile(ref TumpTiles, Singularwidth, ref Singularheight, ref Modiheight, 0, Tilewidth, XAdd, ref XSize, ref YSize, ref X, ref Y, ref Tilestart);
			        TempTiles.AddRange(TumpTiles);
                    TumpTiles=new List<List<Byte>>();
                    times += 1;
                }
            }
            OAM = OAMGen(TempTiles, XSize, YSize, X, Y, Tilestart, Tilewidth,Tileheight).ToArray();
            Limit = TempTiles.Count();
            Byte[,] Newtiles=new Byte[Limit, 32];
            for (int i = 0; i < Limit; i++) {
                for (int k = 0; k < 32; k++)
                    Newtiles[i, k] = TempTiles[i][k];
            }
            return Newtiles;
        }
		static void MoveTile(ref List<List<Byte>> TempTiles, int Tilewidth, ref int TileheightMod, ref int Modiheight, int Limitimes, int RealTileWidth, int XAdd, ref List<int> XSize, ref List<int> YSize, ref List<int> X, ref List<int> Y, ref List<int> Tilestart)
		{
            int mors, OriginalStart=Tilestart[Tilestart.Count()-1];
		for (mors = TileheightMod; (mors % 2) != 0; mors++);
            int morn = (mors / 8) * 2;
			int Tileheight=TileheightMod;
            int XTemp = X[X.Count() - 1];
            if (((Tilewidth % 8) == 5) || ((Tilewidth % 8) == 6))
            {
                if(morn>0)
                XSize[XSize.Count() - 1] -= 2;
                YSize[YSize.Count() - 1] -= morn;
                int YOriginal = Y[Y.Count() - 1] + YSize[YSize.Count() - 1];
                for (int g = 0; g < 3; g++)
                {
                    YSize.Add(morn);
                    Y.Add(YOriginal);
                    Tilestart.Add(6+((g*morn)*8)+OriginalStart);
                    XSize.Add(2);
                    X.Add(XTemp + (g * 2));
                    for (int i = 0; i < morn; i++)
                {
                            for (int k = 0; k < 2; k++)
                                TempTiles[((i + (g*morn)) * 8) + k + 6] = TempTiles[((i+(morn*3) ) * 8) +(g*2)+ k];
                    }
                }
                Modiheight -= morn;
				TileheightMod-=morn;
            }
            else if (((Tilewidth % 8) == 3) || ((Tilewidth % 8) == 4))
            {
                if (mors / 2 > 0) {
                    XSize[XSize.Count() - 1] = 4;
                    YSize[YSize.Count() - 1] -= mors / 2;
                    X.Add(X[X.Count() - 1]);
                    Y.Add(Y[Y.Count()-1]+mors / 2);
                    XSize.Add(4);
                    YSize.Add(mors / 2);
                    Tilestart.Add(4+ OriginalStart);
                for (int i = 0; i < mors / 2; i++)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            TempTiles[(i * 8) + k + 4] = TempTiles[((i + (mors / 2)) * 8) + k];
                        }
                    }
                    Modiheight -= mors / 2;
                    TileheightMod -= mors / 2; }
            }
            else if (((Tilewidth % 8) == 2)||((Tilewidth%8)==1))
            {
				morn=mors/4;
                int OriginalY = Y[Y.Count()-1];
                int Countemp = X.Count() - 1;
				if((mors%4)!=0)
					morn++;
                XSize[Countemp] = 2;
                for (int g = 0; g < 3; g++)
                {
                    Tilestart.Add(OriginalStart+(2 * (g + 1)));
                    XSize.Add(2);
                    YSize.Add(0);
                    X.Add(XTemp);
                    Y.Add(OriginalY - ((2 - g) * morn));
                    for (int i = 0; i < morn; i++)
                    {
						if((i + (morn * (g+1)))<Tileheight){
							Modiheight -=1;
							TileheightMod-=1;
                            YSize[Countemp] -= 1;
                            YSize[YSize.Count() - 1] += 1;
						}
                        for (int k = 0; k < 2; k++){
							if((i + (morn * (g+1)))<Tileheight)
                            TempTiles[(i * 8) + (g*2) + k + 2] = TempTiles[((i + (morn * (g+1))) * 8) + k];
						else{
							for(int j=0; j<32; j++)
							TempTiles[(i * 8) + (g*2) + k + 2][j] =0;
							}
						}
                    }
                }
			}
		}
        static List<List<Byte>> FixTile(List<List<Byte>> TempTiles, int Tilewidth, int RealTileWidth, ref int Tileheight, ref int Modiheight, ref List<int> XSize, ref List<int> YSize)
		{
			int l=Tilewidth;
			List<List<Byte>>NewTile=new List<List<Byte>>();
            if (RealTileWidth > 4)
            {
                l = 8;
                XSize[XSize.Count() - 1] = 8;
            }
            else if (RealTileWidth > 2)
            {
                l = 4;
                XSize[XSize.Count() - 1] = 4;
            }
			for(int i=0; i<Tileheight; i++){
			for(int k=0; k<l; k++){
				NewTile.Add(new List<Byte>());
                    if (k >= Tilewidth)
                    {
                        for (int j = 0; j < 32; j++)
                            NewTile[(i * l) + k].Add(0);
                    }
                    else
                        NewTile[(i * l) + k]=TempTiles[(i * Tilewidth) + k];
				}
			}
			while(Tileheight%2!=0){
				for(int k=0; k<l; k++){
					NewTile.Add(new List<Byte>());
					for(int j=0; j<32; j++)
						NewTile[(Tileheight*l)+k].Add(0);
				}
				Tileheight+=1;
				Modiheight+=1;
                YSize[YSize.Count() - 1] += 1;
			}
			return NewTile;
		}
		static List<Byte> OAMGen(List<List<Byte>> Tile, List<int> XSize, List<int> YSize, List<int> X, List<int> Y, List<int>Tilestart, int OriginalWidth, int OriginalHeight)
        {
            List<Byte> OAMList = new List<byte>();
            Byte Shape = 0, size = 0;
            int l = OriginalWidth;
            if (OriginalWidth > 4)
                l = 8;
            else if (OriginalWidth > 2)
                l = 4;
            for (int k = 0; k < (Tilestart.Count()); k++) {
                if (Tilestart[k] >= 1024)
                {
                    Console.WriteLine("Too many tiles in the end!");
                    Environment.Exit(01);
                }
                int Height, Tempheight=0, Width=0, TempWidth=0; 
                int y = 0;
                for(int i = YSize[k]; i > 0;) {
                    int x = 0;
                    Height = 272 - ((OriginalHeight << 3) - ((Y[k]) << 3)-((YSize[k]-i)<<3));
                    Tempheight = 0;
                    if (i >= 8)
                    {
                        for (int j = XSize[k]; j > 0;)
                        {
                            Width = 400 + ((OriginalWidth / 2) << 3) + (X[k] << 3) + ((XSize[k] - j) << 3);
                            if (j >= 7)
                            {
                                size = 0xC0;
                                Shape = 0;
                                j = 0;
                                i -= 8;
                            }
                            else if (j >= 5)
                            {
                                size = 0xC0;
                                Shape = 0x80;
                                OAMList.Add((Byte)Height);
                                OAMList.Add(Shape);
                                OAMList.Add((Byte)(((Width)) & 0xFF));
                                OAMList.Add((Byte)((size) + ((((Width)) >> 8) & 0x1)));
                                OAMList.Add((Byte)((Tilestart[k]+(y*l)+x)&0xFF));
                                OAMList.Add((Byte)(((Tilestart[k]+(y*l)+x)>>8) & 0x3));
                                OAMList.Add(0);
                                OAMList.Add(0);
                                size = 0x80;
                                Shape = 0x80;
                                OAMList.Add((Byte)(Height));
                                OAMList.Add(Shape);
                                OAMList.Add((Byte)(((Width)+32)&0xFF));
                                OAMList.Add((Byte)((size)+((((Width)+32)>>8) & 0x1)));
                                OAMList.Add((Byte)((Tilestart[k] + (y * l)+4+x) & 0xFF));
                                OAMList.Add((Byte)(((Tilestart[k] + (y * l)+4+x) >> 8) & 0x3));
                                OAMList.Add(0);
                                OAMList.Add(0);
                                size = 0x80;
                                Shape = 0x80;
                                Tempheight += 32;
                                TempWidth += 32;
                                j -= 6;
                                i -= 8;
                                x += 4;
                                y += 4;
                            }
                            else if (j >= 3)
                            {
                                size = 0xC0;
                                Shape = 0x80;
                                j -= 4;
                                i -= 8;
                            }
                            else if (j == 2)
                            {
                                size = 0x80;
                                Shape = 0x80;
                                j -= 2;
                                i -= 4;
                            }
                            else
                            {
                                size = 0x40;
                                Shape = 0x80;
                                j -= 1;
                                i -= 4;
                            }
                            OAMList.Add((Byte)(Height+Tempheight));
                            OAMList.Add(Shape);
                            OAMList.Add((Byte)(((Width) + TempWidth) & 0xFF));
                            OAMList.Add((Byte)((size) + ((((Width) + TempWidth) >> 8) & 0x1)));
                            OAMList.Add((Byte)((Tilestart[k] + (y * l)+x) & 0xFF));
                            OAMList.Add((Byte)(((Tilestart[k] + (y * l)+x) >> 8) & 0x3));
                            OAMList.Add(0);
                            OAMList.Add(0);
                            if ((XSize[k] >= 5 && XSize[k] <= 6)||(XSize[k] == 2)||(XSize[k]==1))
                                y += 4;
                            else
                            y += 8;
                        }
                    }
                    else if (i >= 4)
                    {
                        for (int j = XSize[k]; j > 0;)
                        {
                            Width = 400 + ((OriginalWidth / 2) << 3) + (X[k] << 3) + ((XSize[k] - j) << 3);
                            int xtemp = 0;
                            if ((j == 1))
                            {
                                size = 0x40;
                                Shape = 0x80;
                                j -= 1;
                            }
                            else if (j == 2)
                            {
                                size = 0x80;
                                Shape = 0x80;
                                j = 0;
                            }
                            else if(j>=7)
                            {
                                size = 0xC0;
                                Shape = 0x40;
                                j -= 8;
                            }
                            else
                            {
                                size =0x80;
                                Shape = 0;
                                j -= 4;
                                xtemp += 4;
                            }
                            OAMList.Add((Byte)Height);
                            OAMList.Add(Shape);
                            OAMList.Add((Byte)(((Width)) & 0xFF));
                            OAMList.Add((Byte)((size) + ((((Width)) >> 8) & 0x1)));
                            OAMList.Add((Byte)((Tilestart[k] + (y * l) + x) & 0xFF));
                            OAMList.Add((Byte)(((Tilestart[k] + (y * l) + x) >> 8) & 0x3));
                            OAMList.Add(0);
                            OAMList.Add(0);
                            x += xtemp;
                          }
                        i -= 4;
                        y += 4;
                    }
                    else if ((i == 1) || (i == 3))
                    {
                        for (int j = XSize[k]; j > 0;)
                        {
                            Width = 400 + ((OriginalWidth / 2) << 3) + (X[k] << 3) + ((XSize[k] - j) << 3);
                            int xtemp = 0;
                            if (j == 1)
                            {
                                size = 0;
                                Shape = 0;
                                j -= 1;
                            }
                            else if (j == 2)
                            {
                                size = 0;
                                Shape = 0x40;
                                j = 0;
                            }
                            else
                            {
                                size = 0x40;
                                Shape = 0x40;
                                j -= 4;
                                xtemp += 4;
                            }
                            OAMList.Add((Byte)Height);
                            OAMList.Add(Shape);
                            OAMList.Add((Byte)(((Width)) & 0xFF));
                            OAMList.Add((Byte)((size) + ((((Width)) >> 8) & 0x1)));
                            OAMList.Add((Byte)((Tilestart[k] + (y * l) + x) & 0xFF));
                            OAMList.Add((Byte)(((Tilestart[k] + (y * l) + x) >> 8) & 0x3));
                            OAMList.Add(0);
                            OAMList.Add(0);
                            x += xtemp;
                        }
                        i -= 1;
                        y += 1;
                    }
                   else if (i == 2)
                    {
                        for (int j = XSize[k]; j > 0;)
                        {
                            Width = 400 + ((OriginalWidth / 2) << 3) + (X[k] << 3) + ((XSize[k] - j) << 3);
                            int xtemp = 0;
                            if ((j == 1))
                            {
                                size = 0;
                                Shape = 0x80;
                                j -= 1;
                            }
                            else if (j == 2)
                            {
                                size = 0x40;
                                Shape = 0;
                                j = 0;
                            }
                            else
                            {
                                size = 0x80;
                                Shape = 0x40;
                                j -= 4;
                                xtemp += 4;
                            }
                            OAMList.Add((Byte)Height);
                            OAMList.Add(Shape);
                            OAMList.Add((Byte)(((Width)) & 0xFF));
                            OAMList.Add((Byte)((size) + ((((Width)) >> 8) & 0x1)));
                            OAMList.Add((Byte)((Tilestart[k] + (y * l) + x) & 0xFF));
                            OAMList.Add((Byte)(((Tilestart[k] + (y * l) + x) >> 8) & 0x3));
                            OAMList.Add(0);
                            OAMList.Add(0);
                            x += xtemp;
                        }
                        i -= 2;
                        y += 1;
                    }
                }
            }
            return OAMList;
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
        static List<List<Byte>> RemoveExceedTiles(List<List<Byte>>Tile,int Tilewidth, int Tileheight, out int Tilewdth, out int Tilehight, ref int Modiheight, ref List<int> XSize, ref List<int> YSize, ref List<int> X, ref List<int> Y, ref int XAdd)
        {
            int temp = 0, hu=0, hd=0, hl=0, hr=0, g=0;
            while (g == 0)
            {
                for (int u = (hu*Tilewidth); u < ((hu+1)*Tilewidth); u++)
                {
                    for (int k = 0; k < 32; k++)
                    {
                        if (Tile[u][k] == 0)
                            temp += 1;
                    }
                }
                if (temp == (32 * Tilewidth))
                {
                    hu += 1;
					Modiheight-=1;
                    Y[Y.Count() - 1] += 1;
                    YSize[YSize.Count() - 1] -= 1;
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
                        if (Tile[u][k] == 0)
                            temp += 1;
                    }
                }
                if (temp == (32 * Tilewidth))
                {
                    hd += 1;
					Modiheight-=1;
                    YSize[YSize.Count() - 1] -= 1;
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
                        if (Tile[u][k] == 0)
                            temp += 1;
                    }
                }
                if (temp == (32 * Tileheight))
                {
                    hl += 1;
                    XAdd -= 1;
                    XSize[XSize.Count() - 1] -= 1;
                    X[X.Count()-1] += 1;
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
                        if (Tile[u][k] == 0)
                            temp += 1;
                    }
                }
                if (temp == (32 * (Tileheight)))
                {
                    hr += 1;
                    XAdd += 1;
                    XSize[XSize.Count()-1] -= 1;
                }
                else
                    g = 1;
                temp = 0;
            }
            Tilehight = Tileheight - hu - hd;
            Tilewdth = Tilewidth - hl - hr;
            List<List<Byte>> NewTile = new List<List<Byte>>();
            for(int f=0; f<(Tilehight); f++)
            {
                for (int d = 0; d < Tilewdth; d++)
                {
					NewTile.Add(new List<Byte>());
                        NewTile[(d + (Tilewdth*f))] = Tile[hl +d+(Tilewidth*(f + hu))];
                }
            }
            return NewTile;
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
            Byte[] OAMFront, OAMBack, SOB;
            int tilewidth1 = tilewidth;
            int tileheight1 = 0;
            int tilewidth2 = tilewidth;
            int tileheight2 = 0;
            int Tiletemph = tileheight;
            int Tiletempw = tilewidth;
            if (Back == 1)
            {
                NewTile = SeparateBack(NewTile, tilewidth, tileheight, Back, out tileheight1, out tileheight2, out Tileback);
                NewTile = RemoveExceedingTiles(NewTile, tilewidth, tileheight1, out tilewidth1, out tileheight1);
                Tileback = RemoveExceedingTiles(Tileback, tilewidth2, tileheight2, out tilewidth2, out tileheight2);
                Tileback = TileOAM(Tileback, tileheight2, tilewidth2, Back, out tileheight2, out tilewidth2, out OAMBack);
                OAMTile = TileOAM(NewTile, tileheight1, tilewidth1, Back, out tileheight1, out tilewidth1, out OAMFront);
                OAMTile = UniteTile(OAMTile, Tileback, Back, tilewidth1, tileheight1, tilewidth2, tileheight2, out tilewidth, out tileheight);
                SOB = SOBGen(OAMFront, OAMBack, tileheight1, tilewidth1, Back);
            }
            else
            {
                OAMTile = TileOAM(NewTile, tileheight, tilewidth, Back, out tileheight, out tilewidth , out OAMFront);
                OAMBack = new Byte[0];
                SOB = SOBGen(OAMFront, OAMBack, tileheight, tilewidth, Back);
            }
            OAMTile = Finalize(OAMTile, ref SOB, tilewidth);
            File.WriteAllBytes("SOBBlockTest.bin", SOB);
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
            int Tiletemp = Image.Count()/32;
            Image = GBA.LZ77.Compress(Image);
            byte[] CCG = new byte[Image.Count() + 16];
            CCG[0] = 0x63;
            CCG[1] = 0x63;
            CCG[2] = 0x67;
            CCG[3] = 0x20;
            CCG[4] = 0x2;
            CCG[5] = 0;
            CCG[6] = 0;
            CCG[7] = 0;
            CCG[8] = ((byte)(Tiletemp & 0xFF));
            CCG[9] = ((byte)((Tiletemp>>8) & 0xFF));
            CCG[10] = ((byte)((Tiletemp >> 16) & 0xFF));
            CCG[11] = ((byte)((Tiletemp >> 24) & 0xFF));
            for (int o = 0; o < Image.Count(); o++)
                CCG[o + 12] = Image[o];
            CCG[Image.Count() + 12] = 0x7E;
            CCG[Image.Count() + 13] = 0x63;
            CCG[Image.Count() + 14] = 0x63;
            CCG[Image.Count() + 15] = 0x67;
            File.WriteAllBytes("Testimage_c.bin", CCG);
            return;
        }
    }
}
