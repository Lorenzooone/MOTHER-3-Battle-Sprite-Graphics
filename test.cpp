#include <stdlib.h>
#include <iostream>
#include <fstream>
#include <string>
#include <sstream>
#include "LZ77.h"
bool ends_with(std::string const &a, std::string const &b);

using namespace std;


int main() {
	streampos size;
	int i = 0, f = 0, t = 0, g = 0, d = 0, character = 0;
	unsigned int Num = 0;
	unsigned char option;
	char *memblock, *out, *out2;
	option = 0;
	string input, end=".bin", neo; 
	input = "";
	getline(cin, input);
	while (f == 0) { //Check if the name is valid or not
		input = "";
		cout << "\nType the ROM's name, to end the typing, press Enter.\n";
		getline(cin, input);
		if (ends_with(input, end)) {
			f = 1;
		}
		else
			cout << "\nName not ending in .gba, retry entering the name.\n";
	}
	neo = input+".bak";
	ifstream file(input, ios::in | ios::binary | ios::ate);

	if (file.is_open())
	{
		size = file.tellg();
		memblock = new char[size];
		file.seekg(0, ios::beg);
		file.read(memblock, size);
		file.close();
		ofstream file3(neo, ios::out | ios::binary);
		file3.write(memblock, size);
		file3.close();
		ofstream file2(input, ios::out | ios::binary);
		i=((unsigned char)memblock[character]+((unsigned char)memblock[character+1]<<8)+((unsigned char)memblock[character+2]<<16));
		out=new char[i];
		out2=new char[i];
		i=Decompress(memblock,character,out);
		f=Compress(out, character, i, out2);
		file2.write(out2, f);
		file2.close();
		cout << "\nSuccess!\n";
		delete[] memblock;
	}
	else
	{
		cout << "\nUnable to open file!";
		return 1;
	}
	return 0;
}

bool ends_with(string const &a, string const &b) {
	auto len = b.length();
	auto pos = a.length() - len;
	if (pos < 0)
		return false;
	auto pos_a = &a[pos];
	auto pos_b = &b[0];
	while (*pos_a)
		if (*pos_a++ != *pos_b++)
			return false;
	return true;
}
