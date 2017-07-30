/////////////////////////////////////////////////////////////
////These instructions must be put in m3hacks.asm
//org $804A380; dd $09FD0101 //Change address loaded
//org $804A2EA; dd $00004925 //Load the address that we want to go to in r1
//org $804A2EC; bx r1 //Go to it
//org $804A2F0; mov r5,#0 //Reset r5
//org $9FD0100
//incsrc testArr2.asm
/////////////////////////////////////////////////////////////

//This is the real testArr2.asm
flag_reset:
ldr r1,=#0x2003F00 								//Flag
strb r5,[r1,#0]									//Reset the flag
mov r1,r0										//Do the same things as the original code
lsl r1,r1,#0x10
lsr r1,r1,#0x10
ldr r0,=#0x9B8FFC0
ldr r5,=#0x804A2F1								//Return to the cycle
bx r5