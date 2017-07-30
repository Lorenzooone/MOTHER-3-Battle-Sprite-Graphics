/////////////////////////////////////////////////////////////
////To remove the various entries put these:
//org $8042E70; ldr r0,=#0x8042E8D; bx r0 //If you only want to remove the "Text Speed:" line
//
//org $8042E28; ldr r0,=#0x8042E45; bx r0 //Removes "Favourite Food:"
//org $8042DD6; ldr r0,=#0x8042DF9; bx r0 //Removes "Favourite Thing:"
//
//org $8042E70; ldr r0,=#0x8042EB3; bx r0//If you want to remove Both "Text Speed:" and "Fast/Medium/Slow"
//////////////////////////////////////////////////////////////
//Example for a entry table for the speed arrangements
//org $9FD0200; dd $00000002
//org $9FD0204; dd $0000000C
//org $9FD0208; dd $0000080C
//org $9FD020C; incbin summary_arrangementFAST.bin
//org $9FD0A0C; incbin summary_arrangementSLOW.bin
/////////////////////////////////////////////////////////////
////These instructions must be put in m3hacks.asm for the "Is this Okay?[BREAK]Yes No" printing flag to be reset
//org $804A380; dd $09FD0101 //Change address loaded
//org $804A2EA; dd $00004925 //Load the address that we want to go to in r1
//org $804A2EC; bx r1 //Go to it
//org $804A2F0; mov r5,#0 //Reset r5
//org $9FD0100
//incsrc testArr2.asm
/////////////////////////////////////////////////////////////

//This is the real testArr2.asm
.flag_reset:
cmp r0,#0x4F
bne .NotCycle
ldr r1,=#0x2003F00 								//Flag
strb r5,[r1,#0]									//Reset the flag
ldr r0,=#0x2004CE0								//Check speed for the arrangement... If you don't want this, then edit it away, put a b.medium
ldrb r1,[r0,#0]
cmp r1,#0
beq .fast
cmp r1,#1
beq .medium
b .slow

.fast:
mov r1,#0
ldr r0,=#0x9FD0200
b .ending

.medium:
mov r1,#0x4F
ldr r0,=#0x9B8FFC0
b .ending

.slow:
mov r1,#1
ldr r0,=#0x9FD0200
b .ending

.NotCycle:
mov r1,r0
ldr r0,=#0x9B8FFC0
lsl r1,r1,#0x10
lsr r1,r1,#0x10
b .ending

.ending:
ldr r5,=#0x804A2F1								//Return to the cycle
bx r5