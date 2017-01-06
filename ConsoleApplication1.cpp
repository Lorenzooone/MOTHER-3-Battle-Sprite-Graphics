// ConsoleApplication1.cpp : definisce il punto di ingresso dell'applicazione console.
//
// reading an entire binary file

#include "stdafx.h"
#include <stdlib.h>
#include <iostream>
#include <fstream>
#include <string>
#include <sstream>
#include <time.h>
bool ends_with(std::string const &a, std::string const &b);

using namespace std;

unsigned int GRAPH[257][1500], FINALGRAPH[257][1500]; //Based on original MOTHER 3's enemy CCG blocks, maximum is around 1200

int main() {
	srand(time(NULL));
	streampos size;
	int i = 0, f = 0, t = 0, g = 0, d = 0, character = 0;
	unsigned int Num = 0, GIGAMATR[257][52], LenghtMATR[257], LenghtMATR2[257], UNIMATR[257][52], actual = 0, Num2 = 0, Corrispondenze[257], Relazioni[257], Pointers[257], FinalPointers[257], Array1[4], Freed = 0, Corrispondenze2[257], GraphPointers[257], FinalGraphPointers[257];
	unsigned char option;
	char *memblock;
	for (f = 0; f <= 3; f++) {
		Array1[f] = 0;
	}
	for (g = 0; g <= 256; g++) {
		for (f = 0; f <= 51; f++) {
			GIGAMATR[g][f] = 0;
			UNIMATR[g][f] = 0;
		}
	}
	for (g = 0; g <= 256; g++) {
		for (f = 0; f <= 1499; f++) {
			GRAPH[g][f] = 0;
			FINALGRAPH[g][f] = 0;
		}
	}
	for (f = 0; f <= 256; f++) {
		Relazioni[f] = 0;
		Corrispondenze2[f]=0;
		Corrispondenze[f] = 0;
		LenghtMATR[f] = 0;
		LenghtMATR2[f] = 0;
		Pointers[f] = 0;
		FinalPointers[f] = 0;
		GraphPointers[f] = 0;
		FinalGraphPointers[f] = 0;
	}
	g = 0;
	f = 0;
	string input, end = ".gba", neo; //ROM Name
	i = 0;
	f = 0;
	d = 0;
	option = 0;
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
	neo = input + ".bak";
	ifstream file(input, ios::in | ios::binary | ios::ate);

	if (file.is_open())
	{
		size = file.tellg();
		if ((int)size != 33554432) {
			cout << "\nError! ROM not of 32MB... Perhaps bad dump? Shutting down everything.";
			return 1;
		} //Initialize them all
		memblock = new char[size];
		file.seekg(0, ios::beg);
		file.read(memblock, size);
		file.close();
		ofstream file3(neo, ios::out | ios::binary);

		file3.write(memblock, size);
		file3.close();
		ofstream file2(input, ios::out | ios::binary);
		i = 29957768;//Start of enemy SOB blocks pointers
		t = 0;
		f = 1;
		character = 0;
		d = 29952352;//Start of battle pointers
		while (t <= 256) {
			Num = (unsigned char)memblock[i] + ((unsigned char)memblock[i+1]*256) + ((unsigned char)memblock[i+2]* 65536) + ((unsigned char)memblock[i+3]* 16777216);
			Pointers[t] = Num; //Memorize original pointers
			g = Num + d;
			f = 1;
			while (f == 1) {
				Num = ((unsigned char)memblock[g] * 16777216) + ((unsigned char)memblock[g + 1] * 65536) + ((unsigned char)memblock[g + 2] * 256) + ((unsigned char)memblock[g + 3] );
				g = g + 4;
				GIGAMATR[t][character] = Num; //Memorize every SOB block
				character = character + 1;
				if (Num == 2121494370) { //See if this is the end of the SOB block, if it isn't, then continue scanning
					f = 0;
					LenghtMATR[t] = character-1;
					character = 0;
				}
			}
			t = t + 1;
			i = i + 8; //Next pointer
		}
		for (f = 0; f <= 51; f++)
			UNIMATR[0][f] = GIGAMATR[0][f]; //First unique SOB block is the one of enemy 00
		LenghtMATR2[0] = LenghtMATR[0];
		Relazioni[0] = 0;
		Corrispondenze[0] = 0;
		f = 0;
		g = 0;
		d = 0;
		t = 1;
		i = 0;
		character = 0;
		while (t <= 256) {
			while (i <= actual) {
				if (LenghtMATR[t] != LenghtMATR2[i])
					i = i + 1; //Do not even waste time comparing blocks if they differ in lenght
				else {
					while (g <= LenghtMATR[t]) {
						Num = UNIMATR[i][g];
						Num2 = GIGAMATR[t][g]; //Compare blocks with the same lenght
						if (Num==Num2)
							d = d + 1; //If a set of four bytes is the same, add one
						g = g + 1;
					}
					if (d == g) { //If in the end g=d, then the blocks are the same
						Corrispondenze[t] = Relazioni[i];
						Corrispondenze2[t] = i; //Set everything up for later repointing
						i = actual + 1; //Exit from the whole inner cycle, this isn't an unique SOB block
						character = character + 1; //Flag this as a non-unique SOB block
					}
					g = 0;
					d = 0;
				i = i + 1;//Get ready for another cycle if i+1<=actual
				}
			}
			i = 0;
			if (character == 0) {//If this is an unique SOB block, then character=0
				actual = actual + 1; //Add one more unique block to the count
				for (f = 0; f <= LenghtMATR[t]; f++)
					UNIMATR[actual][f] = GIGAMATR[t][f]; //Put the unique block in the new matrix
				LenghtMATR2[actual] = LenghtMATR[t];
				Corrispondenze2[t] = actual; //This is unique
				Relazioni[actual] = t;
				Corrispondenze[t] = t;
			}
			character = 0;//Set everything back and get ready for another enemy
			t = t + 1;
		}
		t = 0;
		f = 1;
		character = 0;
		d = 29952352; //Start of battle pointers. Remove everything, so that the SOB blocks may be reorganized
		while (t <= 256) {
				g = Pointers[t];
				character = g + d;
				for (g = 0; g <= LenghtMATR[t]; g++) {
					memblock[character] = 255;
					memblock[character + 1] = 255;
					memblock[character + 2] = 255;
					memblock[character + 3] = 255;
					character = character + 4;
					Freed = Freed + 1;
				}
			t = t + 1;
		}
		i = 29952352; //Start of battle pointers
		g = 0;
		d = 0;
		t = 0;
		g = i + Pointers[0]; //Get beginning of enemy SOB blocks
		while (t <= actual) {
			FinalPointers[t] = g-i; //Set the new pointers to unique SOB blocks
			for (f = 0; f <= LenghtMATR2[t]; f++) {
				Num = UNIMATR[t][f]; //Print back each and every block of a SOB block, only then advance
				while (Num >= 16777216) {
					Array1[3] = Array1[3] + 1;
					Num = Num - 16777216;
				}
				while (Num >= 65536) {
					Array1[2] = Array1[2] + 1;
					Num = Num - 65536;
				}
				while (Num >= 256) {
					Array1[1] = Array1[1] + 1;
					Num = Num - 256;
				}
				Array1[0] = Num;
				Freed = Freed - 1;
				for (d = 3; d >= 0; d--) {
					memblock[g + d] = Array1[3-d];
				}
				g = g + 4; //Continue advancing by 4
				for (d = 0; d <= 3; d++)
					Array1[d] = 0;
			}
			t = t + 1; //Next unique SOB block
		}
		i = 29957768;//Start of enemy SOB pointers
		t = 0;
		f = 0;
		character = 0;
		while (t <= 256) {
			d = Corrispondenze2[t]; //Unique SOB blocks point to themselves, all repeated ones point to the first one of them
			Num=FinalPointers[d]; //Print new pointers for every enemy's SOB block
			while (Num >= 16777216) {
				Array1[3] = Array1[3] + 1;
				Num = Num - 16777216;
			}
			while (Num >= 65536){
				Array1[2] = Array1[2] + 1;
			Num = Num - 65536;
			}
		while (Num >= 256){
			Array1[1] = Array1[1] + 1;
		Num = Num - 256;
			}
			Array1[0] = Num;
			for (d = 0; d <= 3; d++)
				memblock[i + d] = Array1[d];
			t = t + 1;
			i = i + 8;
			for (f = 0; f <= 3; f++) {
				Array1[f] = 0;
			}
			f = 0;
		}
		for (f = 0; f <= 256; f++) {
			LenghtMATR[f] = 0;
			LenghtMATR2[f] = 0;
		}
		i = 29952424;//Start of enemy CCG blocks pointers
		actual = 0;
		t = 0;
		f = 1;
		character = 0;
		d = 29952352;//Start of battle pointers
		while (t <= 256) {
			Num = (unsigned char)memblock[i] + ((unsigned char)memblock[i + 1] * 256) + ((unsigned char)memblock[i + 2] * 65536) + ((unsigned char)memblock[i + 3] * 16777216);
			GraphPointers[t] = Num; //Memorize original pointers
			g = Num + d;
			f = 1;
			while (f == 1) {
				Num = ((unsigned char)memblock[g] * 16777216) + ((unsigned char)memblock[g + 1] * 65536) + ((unsigned char)memblock[g + 2] * 256) + ((unsigned char)memblock[g + 3]);
				g = g + 4;
				GRAPH[t][character] = Num; //Memorize every CCG block
				character = character + 1;
				if (Num == 2120442727) { //See if this is the end of the CCG block, if it isn't, then continue scanning
					f = 0;
					LenghtMATR[t] = character - 1;
					character = 0;
				}
			}
			t = t + 1;
			i = i + 8; //Next pointer
		}
		g = LenghtMATR[0];
		for (f = 0; f <= g; f++)
			FINALGRAPH[0][f] = GRAPH[0][f]; //First unique CCG block is the one of enemy 00
		LenghtMATR2[0] = g;
		Relazioni[0] = 0;
		Corrispondenze[0] = 0;
		f = 0;
		g = 0;
		d = 0;
		t = 1;
		i = 0;
		character = 0;
		while (t <= 256) {
			while (i <= actual) {
				if (LenghtMATR[t] != LenghtMATR2[i])
					i = i + 1; //Do not even waste time comparing blocks if they differ in lenght
				else {
					while (g <= LenghtMATR[t]) {
						Num = FINALGRAPH[i][g];
						Num2 = GRAPH[t][g]; //Compare blocks with the same lenght
						if (Num == Num2)
							d = d + 1; //If a set of four bytes is the same, add one
						g = g + 1;
					}
					if (d == g) { //If in the end g=d, then the blocks are the same
						Corrispondenze[t] = Relazioni[i];
						Corrispondenze2[t] = i; //Set everything up for later repointing
						i = actual + 1; //Exit from the whole inner cycle, this isn't an unique CCG block
						character = character + 1; //Flag this as a non-unique CCG block
					}
					g = 0;
					d = 0;
					i = i + 1;//Get ready for another cycle if i+1<=actual
				}
			}
			i = 0;
			if (character == 0) {//If this is an unique CCG block, then character=0
				actual = actual + 1; //Add one more unique block to the count
				for (f = 0; f <= LenghtMATR[t]; f++)
					FINALGRAPH[actual][f] = GRAPH[t][f]; //Put the unique block in the new matrix
				LenghtMATR2[actual] = LenghtMATR[t];
				Corrispondenze2[t] = actual; //This is unique
				Relazioni[actual] = t;
				Corrispondenze[t] = t;
			}
			character = 0;//Set everything back and get ready for another enemy
			t = t + 1;
		}
		t = 0;
		f = 1;
		character = 0;
		d = 29952352; //Start of battle pointers. Remove everything, so that the CCG blocks may be reorganized
		while (t <= 256) {
			g = GraphPointers[t];
			character = g + d;
			for (g = 0; g <= LenghtMATR[t]; g++) {
				memblock[character] = 255;
				memblock[character + 1] = 255;
				memblock[character + 2] = 255;
				memblock[character + 3] = 255;
				character = character + 4;
				Freed = Freed + 1;
			}
			t = t + 1;
		}
		i = 29952352; //Start of battle pointers
		g = 0;
		d = 0;
		t = 0;
		g = i + GraphPointers[0]; //Get beginning of enemy CCG blocks
		while (t <= actual) {
			FinalGraphPointers[t] = g - i; //Set the new pointers to unique CCG blocks
			for (f = 0; f <= LenghtMATR2[t]; f++) {
				Num = FINALGRAPH[t][f]; //Print back each and every block of a CCG block, only then advance
				while (Num >= 16777216) {
					Array1[3] = Array1[3] + 1;
					Num = Num - 16777216;
				}
				while (Num >= 65536) {
					Array1[2] = Array1[2] + 1;
					Num = Num - 65536;
				}
				while (Num >= 256) {
					Array1[1] = Array1[1] + 1;
					Num = Num - 256;
				}
				Array1[0] = Num;
				Freed = Freed - 1;
				for (d = 3; d >= 0; d--) {
					memblock[g + d] = Array1[3 - d];
				}
				g = g + 4; //Continue advancing by 4
				for (d = 0; d <= 3; d++)
					Array1[d] = 0;
			}
			t = t + 1; //Next unique CCG block
		}
		i = 29952424;//Start of enemy CCG pointers
		t = 0;
		f = 0;
		character = 0;
		while (t <= 256) {
			d = Corrispondenze2[t]; //Unique CCG blocks point to themselves, all repeated ones point to the first one of them
			Num = FinalGraphPointers[d]; //Print new pointers for every enemy's CCG block
			while (Num >= 16777216) {
				Array1[3] = Array1[3] + 1;
				Num = Num - 16777216;
			}
			while (Num >= 65536) {
				Array1[2] = Array1[2] + 1;
				Num = Num - 65536;
			}
			while (Num >= 256) {
				Array1[1] = Array1[1] + 1;
				Num = Num - 256;
			}
			Array1[0] = Num;
			for (d = 0; d <= 3; d++)
				memblock[i + d] = Array1[d];
			t = t + 1;
			i = i + 8;
			for (f = 0; f <= 3; f++) {
				Array1[f] = 0;
			}
			f = 0;
		}
		cout << "\nTotal freed blocks: " << (Freed * 4);
		file2.write(memblock, size);
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