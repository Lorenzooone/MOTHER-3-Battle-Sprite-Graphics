#include<iostream>
int Decompress(char *data, int address, char *output)
        {
        	int t, n, o, j, i;
            int start = address;
            if ((unsigned char)data[address++] != 16) return -1; // Check for LZ77 signature
            // Read the block length
            int length = (unsigned char)data[address++];
            length += ((unsigned char)data[address++] << 8);
            length += ((unsigned char)data[address++] << 16);

            int bPos = 0;
            while (bPos < length)
            {
                unsigned char ch = (unsigned char)data[address++];
                for (i = 0; i < 8; i++)
                {
                    switch ((ch >> (7 - i)) & 1)
                    {
                        case 0:
                            // Direct copy
                            if (bPos < length){
                            output[bPos++]=(unsigned char)data[address++];
                        }
                            break;

                        case 1:

                            // Compression magic
                            t = ((unsigned char)data[address++] << 8);
                            t += (unsigned char)data[address++];
                            n = ((t >> 12) & 15) + 3;    // Number of bytes to copy
                            o = (t & 4095);

                            // Copy n bytes from bPos-o to the output
                            for (j = 0; j < n; j++)
                            {
                                if (bPos < length){
                                output[bPos] = (unsigned char)output[bPos - o - 1];
                                bPos++;
								}
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            return length;
        }
        int Compress(char *data, int address, int length, char *obuf)
        {
            int start = address, t, d,c;
            int control = 0;
            // Let's start by encoding the signature and the length
            char *tbuf=new char [length];
            obuf[0]=16;
            obuf[1]=(length & 255);
            obuf[2]=((length >> 8) & 255);
            obuf[3]=((length >> 16) & 255);
            int g=4;
            while ((address - start) < length)
            {
			for(int t=0;t<length;t++)
			tbuf[t]=0;
			d=0;
                control = 0;
                for (int i = 0; i < 8; i++)
                {
                    bool found = false;

                    // First byte should be raw
                    if (address == start)
                    {
                        tbuf[d]=(unsigned char)data[address++];
                        d=d+1;
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

                        for (int k = 1; k <= 4096; k++)
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
                            int t = (max_distance - 1) & 4095;
                            t |= (((max_length - 3) & 15) << 12);
                            tbuf[d]=((t >> 8) & 255);
                            tbuf[d+1]=(t & 255);
							d=d+2;
                            // Set the control bit
                            control |= (1 << (7 - i));

                            found = true;
                        }
                    }

                    if (!found)
                    {
                        // If we didn't find any strings, copy the byte to the output
                        tbuf[d]=(unsigned char)data[address++];
                        d=d+1;
                    }
                }

                // Flush the temp buffer
                obuf[g]=(control & 255);
                g=g+1;
                for(c=0; c<d; c++){
                obuf[g]=tbuf[c];
                g=g+1;
            }
            }
            while((g%4)!=0)
            g=g+1;
            return g;
        }


