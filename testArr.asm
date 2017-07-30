//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////These instructions must be put in m3hacks.asm
//org $8042F06; mov r1,#0x9F; lsl r1,r1,#0x14; mov r2,#0x0D0; lsl r2,r2,#0x0C; add r1,r1,r2; add r1,r1,#1; bx r1
//org $9FD0000
//incsrc testArr.asm
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//This is the real testArr.asm

arr_test:
ldr r1, =#0x2003F00 								//Flag
ldrh r2,[r1,#0]
cmp r2,#0
bne + 												//If flag not set, change arrangement
ldr r1, =#0x600F416 								//Arrangement byte to change
mov r2,#0x48										//New byte
mov r3,#33
lsl r3,r3,#8
add r2,r2,r3
strh r2,[r1,#0]
strh r2,[r1,#4]
mov r2,#0x49										//New byte
add r2,r2,r3
strh r2,[r1,#2]
mov r2,#0x4B
add r2,r2,r3
mov r3,#10
add r1,r1,r3
mov r3,#33
lsl r3,r3,#8
strh r2,[r1,#0]
mov r2,#0x68
add r2,r2,r3
mov r3,#54
add r1,r1,r3
mov r3,#33
lsl r3,r3,#8
strh r2,[r1,#0]
mov r2,#0x69
add r2,r2,r3
strh r2,[r1,#2]
mov r2,#0x6A
add r2,r2,r3
strh r2,[r1,#4]
mov r2,#0x63
add r2,r2,r3
strh r2,[r1,#6]
mov r2,#0x64
add r2,r2,r3
strh r2,[r1,#8]
mov r2,#0x6B
add r2,r2,r3
mov r3,#10
add r1,r1,r3
strh r2,[r1,#0]
mov r3,#54
add r1,r1,r3
mov r3,#33
lsl r3,r3,#8
mov r2,#0x5E
add r2,r2,r3
strh r2,[r1,#0]
mov r3,#10
add r1,r1,r3
mov r3,#32
lsl r3,r3,#8
mov r2,#0xF3
add r2,r2,r3
strh r2,[r1,#0]
mov r3,#33
lsl r3,r3,#8
mov r2,#0x33
add r2,r2,r3
strh r2,[r1,#2]
mov r3,#54
add r1,r1,r3
mov r3,#33
lsl r3,r3,#8
mov r2,#0x5D
add r2,r2,r3
strh r2,[r1,#0]
mov r3,#10
add r1,r1,r3
mov r3,#33
lsl r3,r3,#8
mov r2,#0x5F
add r2,r2,r3
strh r2,[r1,#0]
mov r2,#0x13
add r2,r2,r3
strh r2,[r1,#2]
ldr r1, =#0x2003F00 	
mov r2,#1 											//Set flag
strb r2,[r1,#0]
mov r3,#0x1F
+
ldr r1,=#0x8042F17
bx r1 												//Return to normal cycle
