; 🌺 Peony Disassembly → 🌸 Poppy Assembly
; Platform: Atari 2600
; Size: 2048 bytes

.cpu 6502

; Hardware Registers
VSYNC            = $0000
VBLANK           = $0001
WSYNC            = $0002
RSYNC            = $0003
NUSIZ0           = $0004
NUSIZ1           = $0005
COLUP0           = $0006
COLUP1           = $0007
COLUPF           = $0008
COLUBK           = $0009
CTRLPF           = $000a
REFP0            = $000b
REFP1            = $000c
PF0              = $000d
PF2              = $000f
RESP0            = $0010
RESBL            = $0014
AUDC0            = $0015
AUDC1            = $0016
AUDF0            = $0017
AUDF1            = $0018
AUDV0            = $0019
AUDV1            = $001a
GRP0             = $001b
GRP1             = $001c
ENAM0            = $001d
HMP0             = $0020
HMP1             = $0021
VDELP0           = $0025
VDELP1           = $0026
RESMP0           = $0028
HMOVE            = $002a
HMCLR            = $002b
CXCLR            = $002c
VSYNC            = $0040
COLUP1           = $0047
REFP1            = $004c
HMP0             = $0060
HMBL             = $0064
SWCHA            = $0280
SWCHB            = $0282
INTIM            = $0284
TIM64T           = $0296

; === Code Block $f000-$f00e ===
.org $f000

reset:
	sei                      ; $f000: 78
	cld                      ; $f001: d8
	ldx #$ff                 ; $f002: a2 ff
	txs                      ; $f004: 9a
	inx                      ; $f005: e8
	txa                      ; $f006: 8a
	sta $00,x                ; $f007: 95 00 VSYNC
	inx                      ; $f009: e8
	bne $f007                ; $f00a: d0 fb
loc_f00c:
	dec $8c                  ; $f00c: c6 8c
	jmp $f3c6                ; $f00e: 4c c6 f3

; === Code Block $f011-$f018 ===
.org $f011

loc_f011:
	lda ($95),y              ; $f011: b1 95
loc_f013:
	dey                      ; $f013: 88
	sty $f3                  ; $f014: 84 f3
	ldx $9b                  ; $f016: a6 9b

; === Code Block $f018-$f07c ===
.org $f018

loc_f018:
	dec $8d                  ; $f018: c6 8d
	sta $02                  ; $f01a: 85 02 WSYNC
	sta $1b,x                ; $f01c: 95 1b GRP0
	beq $f062                ; $f01e: f0 42
loc_f020:
	ldx $ee                  ; $f020: a6 ee
	lda $8d                  ; $f022: a5 8d
	cmp $d6,x                ; $f024: d5 d6
	beq $f03c                ; $f026: f0 14
loc_f028:
	sec                      ; $f028: 38
	sbc $9a                  ; $f029: e5 9a
	cmp #$14                 ; $f02b: c9 14 RESBL
	tay                      ; $f02d: a8
	lda $ae,x                ; $f02e: b5 ae
	sta $97                  ; $f030: 85 97
	lda $c6,x                ; $f032: b5 c6
	sta $f2                  ; $f034: 85 f2
	bcc $f011                ; $f036: 90 d9
loc_f038:
	lda #$00                 ; $f038: a9 00 VSYNC
	beq $f013                ; $f03a: f0 d7
loc_f03c:
	bit $07                  ; $f03c: 24 07 COLUP1
	bmi $f042                ; $f03e: 30 02
loc_f040:
	stx $ef                  ; $f040: 86 ef
loc_f042:
	lda $9e,x                ; $f042: b5 9e
	ldx $9c                  ; $f044: a6 9c
	sta $20,x                ; $f046: 95 20 HMP0
	sta $0b,x                ; $f048: 95 0b REFP0
	ldy $f3                  ; $f04a: a4 f3
	dec $f3                  ; $f04c: c6 f3
	cpy #$14                 ; $f04e: c0 14 RESBL
	bcc $f056                ; $f050: 90 04
loc_f052:
	lda #$00                 ; $f052: a9 00 VSYNC
	bcs $f058                ; $f054: b0 02
loc_f056:
	lda ($95),y              ; $f056: b1 95
loc_f058:
	ldx $9b                  ; $f058: a6 9b
	ldy $ee                  ; $f05a: a4 ee
	dec $8d                  ; $f05c: c6 8d
	sta $02                  ; $f05e: 85 02 WSYNC
	sta $1b,x                ; $f060: 95 1b GRP0
loc_f062:
	beq $f0ac                ; $f062: f0 48
loc_f064:
	ldx $a6,y                ; $f064: b6 a6
	bmi $f07f                ; $f066: 30 17
loc_f068:
	dex                      ; $f068: ca
	bpl $f068                ; $f069: 10 fd
loc_f06b:
	ldx $9c                  ; $f06b: a6 9c
	sta $10,x                ; $f06d: 95 10 RESP0
	lda $00ce,y              ; $f06f: b9 ce 00
	tay                      ; $f072: a8
	lda $f7bb,y              ; $f073: b9 bb f7
	eor $f9                  ; $f076: 45 f9
	and $ed                  ; $f078: 25 ed
	sta $06,x                ; $f07a: 95 06 COLUP0
	jmp $f096                ; $f07c: 4c 96 f0

; === Code Block $f07f-$f0ac ===
.org $f07f

loc_f07f:
	lda $00ce,y              ; $f07f: b9 ce 00
	tay                      ; $f082: a8
	lda $f7bb,y              ; $f083: b9 bb f7
	eor $f9                  ; $f086: 45 f9
	and $ed                  ; $f088: 25 ed
	ldy $9c                  ; $f08a: a4 9c
	sta $0006,y              ; $f08c: 99 06 00 COLUP0
	dex                      ; $f08f: ca
	bmi $f08f                ; $f090: 30 fd
loc_f092:
	ldx $9c                  ; $f092: a6 9c
	sta $10,x                ; $f094: 95 10 RESP0
loc_f096:
	sta $02                  ; $f096: 85 02 WSYNC
	sta $2a                  ; $f098: 85 2a HMOVE
	ldy $f3                  ; $f09a: a4 f3
	cpy #$14                 ; $f09c: c0 14 RESBL
	bcc $f0a4                ; $f09e: 90 04
loc_f0a0:
	lda #$00                 ; $f0a0: a9 00 VSYNC
	bcs $f0a6                ; $f0a2: b0 02
loc_f0a4:
	lda ($95),y              ; $f0a4: b1 95
loc_f0a6:
	ldx $9b                  ; $f0a6: a6 9b
	sta $1b,x                ; $f0a8: 95 1b GRP0
	dec $8d                  ; $f0aa: c6 8d

; === Code Block $f0ac-$f100 ===
.org $f0ac

loc_f0ac:
	beq $f105                ; $f0ac: f0 57
loc_f0ae:
	ldx $ee                  ; $f0ae: a6 ee
	ldy $b6,x                ; $f0b0: b4 b6
	ldx $9c                  ; $f0b2: a6 9c
	bpl $f0ce                ; $f0b4: 10 18
loc_f0b6:
	bcc $f0ba                ; $f0b6: 90 02
loc_f0b8:
	eor #$05                 ; $f0b8: 49 05 NUSIZ1
loc_f0ba:
	ldy $f3                  ; $f0ba: a4 f3
	sta $02                  ; $f0bc: 85 02 WSYNC
	sta $04,x                ; $f0be: 95 04 NUSIZ0
	cpy #$14                 ; $f0c0: c0 14 RESBL
	bcc $f0ea                ; $f0c2: 90 26
loc_f0c4:
	lda #$00                 ; $f0c4: a9 00 VSYNC
	bcs $f0ec                ; $f0c6: b0 24
loc_f0c8:
	lda ($97),y              ; $f0c8: b1 97
	ldx $9c                  ; $f0ca: a6 9c
	sta $1b,x                ; $f0cc: 95 1b GRP0
loc_f0ce:
	sty $f4                  ; $f0ce: 84 f4
	dec $f3                  ; $f0d0: c6 f3
	lda $f2                  ; $f0d2: a5 f2
	cpy #$10                 ; $f0d4: c0 10 RESP0
	bne $f0b6                ; $f0d6: d0 de
loc_f0d8:
	sta $20,x                ; $f0d8: 95 20 HMP0
	ldy $f3                  ; $f0da: a4 f3
	cpy #$14                 ; $f0dc: c0 14 RESBL
	sta $02                  ; $f0de: 85 02 WSYNC
	sta $2a                  ; $f0e0: 85 2a HMOVE
	sta $04,x                ; $f0e2: 95 04 NUSIZ0
	bcc $f0ea                ; $f0e4: 90 04
loc_f0e6:
	lda #$00                 ; $f0e6: a9 00 VSYNC
	bcs $f0ec                ; $f0e8: b0 02
loc_f0ea:
	lda ($95),y              ; $f0ea: b1 95
loc_f0ec:
	ldx $9b                  ; $f0ec: a6 9b
	sta $1b,x                ; $f0ee: 95 1b GRP0
	dec $8d                  ; $f0f0: c6 8d
	beq $f103                ; $f0f2: f0 0f
loc_f0f4:
	ldy $f4                  ; $f0f4: a4 f4
	dey                      ; $f0f6: 88
	bpl $f0c8                ; $f0f7: 10 cf
loc_f0f9:
	inc $ee                  ; $f0f9: e6 ee
	ldy $f3                  ; $f0fb: a4 f3
	dey                      ; $f0fd: 88
	cpy #$14                 ; $f0fe: c0 14 RESBL
	jmp $f036                ; $f100: 4c 36 f0

; === Code Block $f103-$f105 ===
.org $f103

loc_f103:
	inc $ee                  ; $f103: e6 ee

; === Code Block $f105-$f171 ===
.org $f105

loc_f105:
	sta $02                  ; $f105: 85 02 WSYNC
	ldx #$00                 ; $f107: a2 00 VSYNC
	stx $25                  ; $f109: 86 25 VDELP0
	stx $26                  ; $f10b: 86 26 VDELP1
	stx $1c                  ; $f10d: 86 1c GRP1
	stx $1b                  ; $f10f: 86 1b GRP0
	stx $0b                  ; $f111: 86 0b REFP0
	stx $0c                  ; $f113: 86 0c REFP1
	lda #$38                 ; $f115: a9 38
	jsr $f691                ; $f117: 20 91 f6
loc_f11a:
	lda #$3f                 ; $f11a: a9 3f
	inx                      ; $f11c: e8
	jsr $f691                ; $f11d: 20 91 f6
loc_f120:
	ldx #$09                 ; $f120: a2 09 COLUBK
	lda $f7c3,x              ; $f122: bd c3 f7
	sta $81,x                ; $f125: 95 81
	dex                      ; $f127: ca
	bpl $f122                ; $f128: 10 f8
loc_f12a:
	jsr $f4f8                ; $f12a: 20 f8 f4
loc_f12d:
	ldy #$25                 ; $f12d: a0 25 VDELP0
	sty $0296                ; $f12f: 8c 96 02 TIM64T
	ldy $91                  ; $f132: a4 91
	beq $f174                ; $f134: f0 3e
loc_f136:
	sta $16                  ; $f136: 85 16 AUDC1
	sta $df                  ; $f138: 85 df
	bmi $f171                ; $f13a: 30 35
loc_f13c:
	cpy #$0a                 ; $f13c: c0 0a CTRLPF
	bcs $f16f                ; $f13e: b0 2f
loc_f140:
	lda $0280                ; $f140: ad 80 02 SWCHA
	dey                      ; $f143: 88
	beq $f16b                ; $f144: f0 25
loc_f146:
	jsr $f59c                ; $f146: 20 9c f5
loc_f149:
	ldx $91                  ; $f149: a6 91
	lda $f7f2,x              ; $f14b: bd f2 f7
	sta $dd                  ; $f14e: 85 dd
	and #$80                 ; $f150: 29 80
	ora $bd                  ; $f152: 05 bd
	sta $bd                  ; $f154: 85 bd
	ldx $8c                  ; $f156: a6 8c
	ldy #$aa                 ; $f158: a0 aa
	cpx #$05                 ; $f15a: e0 05 NUSIZ1
	bcc $f160                ; $f15c: 90 02
loc_f15e:
	ldy #$0a                 ; $f15e: a0 0a CTRLPF
loc_f160:
	sty $ec                  ; $f160: 84 ec
	lda $f7e6,x              ; $f162: bd e6 f7
	and #$f0                 ; $f165: 29 f0
	sta $eb                  ; $f167: 85 eb
	sta $f6                  ; $f169: 85 f6
loc_f16b:
	cmp #$f0                 ; $f16b: c9 f0
	bcs $f171                ; $f16d: b0 02
loc_f16f:
	dec $91                  ; $f16f: c6 91
loc_f171:
	jmp $f37f                ; $f171: 4c 7f f3

; === Code Block $f174-$f3c6 ===
.org $f174

loc_f174:
	sty $e7                  ; $f174: 84 e7
	sty $e6                  ; $f176: 84 e6
	clc                      ; $f178: 18
	lda $9d                  ; $f179: a5 9d
	adc #$aa                 ; $f17b: 69 aa
	sta $9d                  ; $f17d: 85 9d
	lda #$01                 ; $f17f: a9 01 VBLANK
	sed                      ; $f181: f8
	adc $ea                  ; $f182: 65 ea
	sta $ea                  ; $f184: 85 ea
	tya                      ; $f186: 98
	jsr $f55e                ; $f187: 20 5e f5
loc_f18a:
	bcc $f18e                ; $f18a: 90 02
loc_f18c:
	stx $91                  ; $f18c: 86 91
loc_f18e:
	lda $07                  ; $f18e: a5 07 COLUP1
	bpl $f1f7                ; $f190: 10 65
loc_f192:
	ldx $ef                  ; $f192: a6 ef
	lda $ce,x                ; $f194: b5 ce
	and #$03                 ; $f196: 29 03 RSYNC
	cmp #$01                 ; $f198: c9 01 VBLANK
	beq $f1ab                ; $f19a: f0 0f
loc_f19c:
	lda $d6,x                ; $f19c: b5 d6
	bcs $f1a4                ; $f19e: b0 04
loc_f1a0:
	sbc #$06                 ; $f1a0: e9 06 COLUP0
	bcc $f1a8                ; $f1a2: 90 04
loc_f1a4:
	sbc $b6,x                ; $f1a4: f5 b6
	bcs $f1a9                ; $f1a6: b0 01
loc_f1a8:
	tya                      ; $f1a8: 98
loc_f1a9:
	cmp $9a                  ; $f1a9: c5 9a
loc_f1ab:
	tya                      ; $f1ab: 98
	bcs $f1b0                ; $f1ac: b0 02
loc_f1ae:
	eor #$01                 ; $f1ae: 49 01 VBLANK
loc_f1b0:
	sta $9b                  ; $f1b0: 85 9b
	eor #$01                 ; $f1b2: 49 01 VBLANK
	sta $9c                  ; $f1b4: 85 9c
	bit $8f                  ; $f1b6: 24 8f
	bvs $f1f7                ; $f1b8: 70 3d
loc_f1ba:
	ldy $ce,x                ; $f1ba: b4 ce
	lda #$f8                 ; $f1bc: a9 f8
	clc                      ; $f1be: 18
	adc $d6,x                ; $f1bf: 75 d6
	sec                      ; $f1c1: 38
	sbc $b6,x                ; $f1c2: f5 b6
	sec                      ; $f1c4: 38
	sbc $9a                  ; $f1c5: e5 9a
	cmp #$f8                 ; $f1c7: c9 f8
	bcc $f1f7                ; $f1c9: 90 2c
loc_f1cb:
	tya                      ; $f1cb: 98
	and #$03                 ; $f1cc: 29 03 RSYNC
	beq $f1f7                ; $f1ce: f0 27
loc_f1d0:
	tay                      ; $f1d0: a8
	dey                      ; $f1d1: 88
	bne $f1d8                ; $f1d2: d0 04
loc_f1d4:
	sty $fa                  ; $f1d4: 84 fa
	beq $f1e3                ; $f1d6: f0 0b
loc_f1d8:
	lda $be,x                ; $f1d8: b5 be
	sbc #$09                 ; $f1da: e9 09 COLUBK
	sec                      ; $f1dc: 38
	sbc $99                  ; $f1dd: e5 99
	cmp #$ef                 ; $f1df: c9 ef
	bcc $f1f7                ; $f1e1: 90 14
loc_f1e3:
	bit $f5                  ; $f1e3: 24 f5
	bmi $f1f9                ; $f1e5: 30 12
loc_f1e7:
	tya                      ; $f1e7: 98
	bne $f1f4                ; $f1e8: d0 0a
loc_f1ea:
	lda $e0                  ; $f1ea: a5 e0
	and #$0f                 ; $f1ec: 29 0f PF2
	bne $f1f7                ; $f1ee: d0 07
loc_f1f0:
	bit $f8                  ; $f1f0: 24 f8
	bpl $f1f7                ; $f1f2: 10 03
loc_f1f4:
	jsr $f575                ; $f1f4: 20 75 f5
loc_f1f7:
	ldy $07                  ; $f1f7: a4 07 COLUP1
loc_f1f9:
	sty $f5                  ; $f1f9: 84 f5
	ldx #$07                 ; $f1fb: a2 07 COLUP1
	lda $ce,x                ; $f1fd: b5 ce
	and #$03                 ; $f1ff: 29 03 RSYNC
	bne $f249                ; $f201: d0 46
loc_f203:
	bit $92                  ; $f203: 24 92
	bpl $f249                ; $f205: 10 42
loc_f207:
	lda $d6,x                ; $f207: b5 d6
	sec                      ; $f209: 38
	sbc $b6,x                ; $f20a: f5 b6
	sec                      ; $f20c: 38
	sbc $9a                  ; $f20d: e5 9a
	sec                      ; $f20f: 38
	sbc #$07                 ; $f210: e9 07 COLUP1
	cmp #$04                 ; $f212: c9 04 NUSIZ0
	bcs $f249                ; $f214: b0 33
loc_f216:
	lda $99                  ; $f216: a5 99
	cmp #$40                 ; $f218: c9 40 VSYNC
	bcs $f226                ; $f21a: b0 0a
loc_f21c:
	lda $be,x                ; $f21c: b5 be
	cmp #$64                 ; $f21e: c9 64 HMBL
	lda $99                  ; $f220: a5 99
	bcc $f226                ; $f222: 90 02
loc_f224:
	adc #$9f                 ; $f224: 69 9f
loc_f226:
	clc                      ; $f226: 18
	sbc #$02                 ; $f227: e9 02 WSYNC
	sec                      ; $f229: 38
	sbc $be,x                ; $f22a: f5 be
	cmp #$1c                 ; $f22c: c9 1c GRP1
	bcc $f239                ; $f22e: 90 09
loc_f230:
	sbc #$20                 ; $f230: e9 20 HMP0
	cmp #$dc                 ; $f232: c9 dc
	bcc $f24c                ; $f234: 90 16
loc_f236:
	jsr $f575                ; $f236: 20 75 f5
loc_f239:
	lda $eb                  ; $f239: a5 eb
	sec                      ; $f23b: 38
	sed                      ; $f23c: f8
	sbc #$01                 ; $f23d: e9 01 VBLANK
	cld                      ; $f23f: d8
	sta $eb                  ; $f240: 85 eb
	lda #$08                 ; $f242: a9 08 COLUPF
	jsr $f58d                ; $f244: 20 8d f5
loc_f247:
	bcs $f24c                ; $f247: b0 03
loc_f249:
	dex                      ; $f249: ca
	bpl $f1fd                ; $f24a: 10 b1
loc_f24c:
	lda $8f                  ; $f24c: a5 8f
	and #$0f                 ; $f24e: 29 0f PF2
	tax                      ; $f250: aa
	lda $0280                ; $f251: ad 80 02 SWCHA
	bit $8f                  ; $f254: 24 8f
	bvs $f25e                ; $f256: 70 06
loc_f258:
	cmp #$c0                 ; $f258: c9 c0
	ldy #$00                 ; $f25a: a0 00 VSYNC
	bcs $f262                ; $f25c: b0 04
loc_f25e:
	ldy $e2                  ; $f25e: a4 e2
	bne $f283                ; $f260: d0 21
loc_f262:
	sty $16                  ; $f262: 84 16 AUDC1
	sty $e2                  ; $f264: 84 e2
	stx $8f                  ; $f266: 86 8f
	asl                      ; $f268: 0a
	bcc $f26f                ; $f269: 90 04
loc_f26b:
	bmi $f287                ; $f26b: 30 1a
loc_f26d:
	dex                      ; $f26d: ca
	dex                      ; $f26e: ca
loc_f26f:
	inx                      ; $f26f: e8
	cpx #$10                 ; $f270: e0 10 RESP0
	bcs $f276                ; $f272: b0 02
loc_f274:
	stx $8f                  ; $f274: 86 8f
loc_f276:
	jsr $f552                ; $f276: 20 52 f5
loc_f279:
	iny                      ; $f279: c8
	iny                      ; $f27a: c8
	lda $e5                  ; $f27b: a5 e5
	bne $f283                ; $f27d: d0 04
loc_f27f:
	lda #$08                 ; $f27f: a9 08 COLUPF
	sta $16                  ; $f281: 85 16 AUDC1
loc_f283:
	dey                      ; $f283: 88
	sty $e2                  ; $f284: 84 e2
	iny                      ; $f286: c8
loc_f287:
	sty $e5                  ; $f287: 84 e5
	jsr $f552                ; $f289: 20 52 f5
loc_f28c:
	lsr                      ; $f28c: 4a
	tax                      ; $f28d: aa
	lda #$a0                 ; $f28e: a9 a0
	bit $8f                  ; $f290: 24 8f
	bvs $f2a6                ; $f292: 70 12
loc_f294:
	lda $f7a4,y              ; $f294: b9 a4 f7
	cmp $8e                  ; $f297: c5 8e
	beq $f2a3                ; $f299: f0 08
loc_f29b:
	bcs $f29f                ; $f29b: b0 02
loc_f29d:
	dec $8e                  ; $f29d: c6 8e
loc_f29f:
	bcc $f2a3                ; $f29f: 90 02
loc_f2a1:
	inc $8e                  ; $f2a1: e6 8e
loc_f2a3:
	lda $f7f8,x              ; $f2a3: bd f8 f7
loc_f2a6:
	sta $95                  ; $f2a6: 85 95
	jsr $f552                ; $f2a8: 20 52 f5
loc_f2ab:
	cmp #$04                 ; $f2ab: c9 04 NUSIZ0
	bcc $f2b8                ; $f2ad: 90 09
loc_f2af:
	ldx #$04                 ; $f2af: a2 04 NUSIZ0
	cpx $8c                  ; $f2b1: e4 8c
	bcs $f2b8                ; $f2b3: b0 03
loc_f2b5:
	lda $f7ec,y              ; $f2b5: b9 ec f7
loc_f2b8:
	adc #$04                 ; $f2b8: 69 04 NUSIZ0
	ldx #$01                 ; $f2ba: a2 01 VBLANK
	asl                      ; $f2bc: 0a
	asl                      ; $f2bd: 0a
	asl                      ; $f2be: 0a
	asl                      ; $f2bf: 0a
	cmp $93,x                ; $f2c0: d5 93
	beq $f2cc                ; $f2c2: f0 08
loc_f2c4:
	bcc $f2c8                ; $f2c4: 90 02
loc_f2c6:
	inc $93,x                ; $f2c6: f6 93
loc_f2c8:
	bcs $f2cc                ; $f2c8: b0 02
loc_f2ca:
	dec $93,x                ; $f2ca: d6 93
loc_f2cc:
	lda $93,x                ; $f2cc: b5 93
	lsr                      ; $f2ce: 4a
	lsr                      ; $f2cf: 4a
	lsr                      ; $f2d0: 4a
	lsr                      ; $f2d1: 4a
	sec                      ; $f2d2: 38
	sbc #$04                 ; $f2d3: e9 04 NUSIZ0
	beq $f2dd                ; $f2d5: f0 06
loc_f2d7:
	bcs $f2db                ; $f2d7: b0 02
loc_f2d9:
	sbc #$01                 ; $f2d9: e9 01 VBLANK
loc_f2db:
	adc #$00                 ; $f2db: 69 00 VSYNC
loc_f2dd:
	sta $ef                  ; $f2dd: 85 ef
	ldy $8c                  ; $f2df: a4 8c
	lda $f7e6,y              ; $f2e1: b9 e6 f7
	lsr                      ; $f2e4: 4a
	lda $8e                  ; $f2e5: a5 8e
	bcs $f2ec                ; $f2e7: b0 03
loc_f2e9:
	adc #$20                 ; $f2e9: 69 20 HMP0
	ror                      ; $f2eb: 6a
loc_f2ec:
	lsr                      ; $f2ec: 4a
	lsr                      ; $f2ed: 4a
	lsr                      ; $f2ee: 4a
	lsr                      ; $f2ef: 4a
	lsr                      ; $f2f0: 4a
	tay                      ; $f2f1: a8
	lda #$00                 ; $f2f2: a9 00 VSYNC
	beq $f2f8                ; $f2f4: f0 02
loc_f2f6:
	adc $ef                  ; $f2f6: 65 ef
loc_f2f8:
	clc                      ; $f2f8: 18
	dey                      ; $f2f9: 88
	bpl $f2f6                ; $f2fa: 10 fa
loc_f2fc:
	iny                      ; $f2fc: c8
	adc $e3,x                ; $f2fd: 75 e3
	bpl $f30d                ; $f2ff: 10 0c
loc_f301:
	cmp #$df                 ; $f301: c9 df
	bcs $f311                ; $f303: b0 0c
loc_f305:
	adc #$20                 ; $f305: 69 20 HMP0
	dey                      ; $f307: 88
	bne $f301                ; $f308: d0 f7
loc_f30a:
	iny                      ; $f30a: c8
	sbc #$20                 ; $f30b: e9 20 HMP0
loc_f30d:
	cmp #$20                 ; $f30d: c9 20 HMP0
	bcs $f30a                ; $f30f: b0 f9
loc_f311:
	sty $f3,x                ; $f311: 94 f3
	sta $e3,x                ; $f313: 95 e3
	clc                      ; $f315: 18
	lda $8f                  ; $f316: a5 8f
	adc #$01                 ; $f318: 69 01 VBLANK
	lsr                      ; $f31a: 4a
	and #$0f                 ; $f31b: 29 0f PF2
	dex                      ; $f31d: ca
	bpl $f2bc                ; $f31e: 10 9c
loc_f320:
	lda #$4c                 ; $f320: a9 4c REFP1
	bit $f8                  ; $f322: 24 f8
	bvs $f334                ; $f324: 70 0e
loc_f326:
	tya                      ; $f326: 98
	ldy #$00                 ; $f327: a0 00 VSYNC
	clc                      ; $f329: 18
	adc $99                  ; $f32a: 65 99
	cmp #$08                 ; $f32c: c9 08 COLUPF
	bcc $f336                ; $f32e: 90 06
loc_f330:
	cmp #$90                 ; $f330: c9 90
	bcs $f336                ; $f332: b0 02
loc_f334:
	sta $99                  ; $f334: 85 99
loc_f336:
	sty $df                  ; $f336: 84 df
	ldx #$07                 ; $f338: a2 07 COLUP1
	clc                      ; $f33a: 18
	lda $92                  ; $f33b: a5 92
	bmi $f344                ; $f33d: 30 05
loc_f33f:
	clc                      ; $f33f: 18
	adc $f4                  ; $f340: 65 f4
	sta $92                  ; $f342: 85 92
loc_f344:
	lda $d6,x                ; $f344: b5 d6
	clc                      ; $f346: 18
	adc $f4                  ; $f347: 65 f4
	bcs $f34d                ; $f349: b0 02
loc_f34b:
	sta $d6,x                ; $f34b: 95 d6
loc_f34d:
	dex                      ; $f34d: ca
	bpl $f344                ; $f34e: 10 f4
loc_f350:
	ldx #$04                 ; $f350: a2 04 NUSIZ0
	cpx $8c                  ; $f352: e4 8c
	bcs $f379                ; $f354: b0 23
loc_f356:
	lda $0c                  ; $f356: a5 0c REFP1
	bit $f8                  ; $f358: 24 f8
	bmi $f35e                ; $f35a: 30 02
loc_f35c:
	lda $fa                  ; $f35c: a5 fa
loc_f35e:
	ldx $e0                  ; $f35e: a6 e0
	bne $f36b                ; $f360: d0 09
loc_f362:
	tay                      ; $f362: a8
	bmi $f379                ; $f363: 30 14
loc_f365:
	lda #$09                 ; $f365: a9 09 COLUBK
	tay                      ; $f367: a8
	jsr $f593                ; $f368: 20 93 f5
loc_f36b:
	ldy #$7c                 ; $f36b: a0 7c
	inx                      ; $f36d: e8
	cpx #$10                 ; $f36e: e0 10 RESP0
	beq $f379                ; $f370: f0 07
loc_f372:
	bcc $f37b                ; $f372: 90 07
loc_f374:
	ldx #$00                 ; $f374: a2 00 VSYNC
	asl                      ; $f376: 0a
	bcc $f37f                ; $f377: 90 06
loc_f379:
	ldy #$78                 ; $f379: a0 78
loc_f37b:
	sty $9a                  ; $f37b: 84 9a
	stx $e0                  ; $f37d: 86 e0
loc_f37f:
	ldy $0284                ; $f37f: ac 84 02 INTIM
	bne $f37f                ; $f382: d0 fb
loc_f384:
	dey                      ; $f384: 88
	sta $02                  ; $f385: 85 02 WSYNC
	sty $00                  ; $f387: 84 00 VSYNC
	sty $01                  ; $f389: 84 01 VBLANK
	sty $ef                  ; $f38b: 84 ef
	ldx #$05                 ; $f38d: a2 05 NUSIZ1
	stx $0a                  ; $f38f: 86 0a CTRLPF
	dex                      ; $f391: ca
	inc $80                  ; $f392: e6 80
	bne $f39c                ; $f394: d0 06
loc_f396:
	inc $e6                  ; $f396: e6 e6
	bne $f39c                ; $f398: d0 02
loc_f39a:
	sty $e7                  ; $f39a: 84 e7
loc_f39c:
	lda $f8                  ; $f39c: a5 f8
	and #$08                 ; $f39e: 29 08 COLUPF
	bne $f3a4                ; $f3a0: d0 02
loc_f3a2:
	ldy #$0f                 ; $f3a2: a0 0f PF2
loc_f3a4:
	tya                      ; $f3a4: 98
	ldy $e7                  ; $f3a5: a4 e7
	bpl $f3ab                ; $f3a7: 10 02
loc_f3a9:
	and #$f7                 ; $f3a9: 29 f7
loc_f3ab:
	sta $ed                  ; $f3ab: 85 ed
	lda $e6                  ; $f3ad: a5 e6
	and $e7                  ; $f3af: 25 e7
	eor $f7fb,x              ; $f3b1: 5d fb f7
	and $ed                  ; $f3b4: 25 ed
	sta $f8,x                ; $f3b6: 95 f8
	sta $05,x                ; $f3b8: 95 05 NUSIZ1
	dex                      ; $f3ba: ca
	bne $f3ad                ; $f3bb: d0 f0
loc_f3bd:
	stx $08                  ; $f3bd: 86 08 COLUPF
	stx $ee                  ; $f3bf: 86 ee
	lda $0282                ; $f3c1: ad 82 02 SWCHB
	sta $f8                  ; $f3c4: 85 f8

; === Code Block $f3c6-$f4f5 ===
.org $f3c6

loc_f3c6:
	ldy #$2c                 ; $f3c6: a0 2c CXCLR
	lsr                      ; $f3c8: 4a
	sta $02                  ; $f3c9: 85 02 WSYNC
	stx $00                  ; $f3cb: 86 00 VSYNC
	sty $0296                ; $f3cd: 8c 96 02 TIM64T
	ldy $8c                  ; $f3d0: a4 8c
	ror                      ; $f3d2: 6a
	bpl $f3dd                ; $f3d3: 10 08
loc_f3d5:
	rol                      ; $f3d5: 2a
	cpy #$05                 ; $f3d6: c0 05 NUSIZ1
	ror                      ; $f3d8: 6a
	ora $0c                  ; $f3d9: 05 0c REFP1
	bmi $f3f6                ; $f3db: 30 19
loc_f3dd:
	lda $f7d9,y              ; $f3dd: b9 d9 f7
	bne $f3e4                ; $f3e0: d0 02
loc_f3e2:
	lda $f7                  ; $f3e2: a5 f7
loc_f3e4:
	sta $f0                  ; $f3e4: 85 f0
	sta $f1                  ; $f3e6: 85 f1
	ldx #$3d                 ; $f3e8: a2 3d
	lda #$00                 ; $f3ea: a9 00 VSYNC
	sta $ad,x                ; $f3ec: 95 ad
	lda $f7ab,x              ; $f3ee: bd ab f7
	sta $8d,x                ; $f3f1: 95 8d
	dex                      ; $f3f3: ca
	bne $f3ea                ; $f3f4: d0 f4
loc_f3f6:
	bcs $f423                ; $f3f6: b0 2b
loc_f3f8:
	dec $8b                  ; $f3f8: c6 8b
	bpl $f425                ; $f3fa: 10 29
loc_f3fc:
	iny                      ; $f3fc: c8
	tya                      ; $f3fd: 98
	cmp #$0a                 ; $f3fe: c9 0a CTRLPF
	bcc $f403                ; $f400: 90 01
loc_f402:
	txa                      ; $f402: 8a
loc_f403:
	sta $8c                  ; $f403: 85 8c
	sed                      ; $f405: f8
	clc                      ; $f406: 18
	adc #$01                 ; $f407: 69 01 VBLANK
	cld                      ; $f409: d8
	sta $eb                  ; $f40a: 85 eb
	stx $e6                  ; $f40c: 86 e6
	stx $e7                  ; $f40e: 86 e7
	ldy $80                  ; $f410: a4 80
	bne $f415                ; $f412: d0 01
loc_f414:
	tay                      ; $f414: a8
loc_f415:
	sty $f7                  ; $f415: 84 f7
	lda #$aa                 ; $f417: a9 aa
	sta $ec                  ; $f419: 85 ec
	sta $ea                  ; $f41b: 85 ea
	sta $e9                  ; $f41d: 85 e9
	sta $91                  ; $f41f: 85 91
	ldx #$1d                 ; $f421: a2 1d ENAM0
loc_f423:
	stx $8b                  ; $f423: 86 8b
loc_f425:
	sec                      ; $f425: 38
	lda $dd                  ; $f426: a5 dd
	sbc #$05                 ; $f428: e9 05 NUSIZ1
	bcc $f439                ; $f42a: 90 0d
loc_f42c:
	sbc $bd                  ; $f42c: e5 bd
	bcc $f439                ; $f42e: 90 09
loc_f430:
	ldx $b6                  ; $f430: a6 b6
	bpl $f439                ; $f432: 10 05
loc_f434:
	sta $de                  ; $f434: 85 de
	jsr $f59c                ; $f436: 20 9c f5
loc_f439:
	ldx $ee                  ; $f439: a6 ee
	ldy $d6,x                ; $f43b: b4 d6
	lda $b6,x                ; $f43d: b5 b6
	bpl $f45b                ; $f43f: 10 1a
loc_f441:
	sbc #$01                 ; $f441: e9 01 VBLANK
	bpl $f44d                ; $f443: 10 08
loc_f445:
	lda #$ff                 ; $f445: a9 ff
	sta $b6,x                ; $f447: 95 b6
	inc $ee                  ; $f449: e6 ee
	bpl $f439                ; $f44b: 10 ec
loc_f44d:
	cmp #$0f                 ; $f44d: c9 0f PF2
	bne $f45a                ; $f44f: d0 09
loc_f451:
	lda #$fb                 ; $f451: a9 fb
	jsr $f656                ; $f453: 20 56 f6
loc_f456:
	sta $be,x                ; $f456: 95 be
	lda #$0f                 ; $f458: a9 0f PF2
loc_f45a:
	dey                      ; $f45a: 88
loc_f45b:
	cpy #$96                 ; $f45b: c0 96
	bcs $f441                ; $f45d: b0 e2
loc_f45f:
	sty $d6,x                ; $f45f: 94 d6
	sta $b6,x                ; $f461: 95 b6
	lda $df                  ; $f463: a5 df
	sec                      ; $f465: 38
	eor #$ff                 ; $f466: 49 ff
	jsr $f657                ; $f468: 20 57 f6
loc_f46b:
	sta $be,x                ; $f46b: 95 be
	jsr $f66e                ; $f46d: 20 6e f6
loc_f470:
	lsr                      ; $f470: 4a
	ora $ce,x                ; $f471: 15 ce
	asl                      ; $f473: 0a
	sta $9e,x                ; $f474: 95 9e
	tya                      ; $f476: 98
	sbc #$04                 ; $f477: e9 04 NUSIZ0
	eor #$80                 ; $f479: 49 80
	bmi $f47e                ; $f47b: 30 01
loc_f47d:
	tya                      ; $f47d: 98
loc_f47e:
	sta $a6,x                ; $f47e: 95 a6
	inx                      ; $f480: e8
	cpx #$08                 ; $f481: e0 08 COLUPF
	bcc $f463                ; $f483: 90 de
loc_f485:
	ldx $e1                  ; $f485: a6 e1
	beq $f493                ; $f487: f0 0a
loc_f489:
	dex                      ; $f489: ca
	bpl $f493                ; $f48a: 10 07
loc_f48c:
	inx                      ; $f48c: e8
	inx                      ; $f48d: e8
	inx                      ; $f48e: e8
	bmi $f493                ; $f48f: 30 02
loc_f491:
	ldx #$0b                 ; $f491: a2 0b REFP0
loc_f493:
	txa                      ; $f493: 8a
	lsr                      ; $f494: 4a
	stx $e1                  ; $f495: 86 e1
	sta $19                  ; $f497: 85 19 AUDV0
	ldx $0284                ; $f499: ae 84 02 INTIM
	bne $f499                ; $f49c: d0 fb
loc_f49e:
	stx $02                  ; $f49e: 86 02 WSYNC
	stx $01                  ; $f4a0: 86 01 VBLANK
	ldx #$04                 ; $f4a2: a2 04 NUSIZ0
	stx $18                  ; $f4a4: 86 18 AUDF1
	ldy #$02                 ; $f4a6: a0 02 WSYNC
	sty $17                  ; $f4a8: 84 17 AUDF0
	dex                      ; $f4aa: ca
	lda $e9,x                ; $f4ab: b5 e9
	and #$0f                 ; $f4ad: 29 0f PF2
	sta $0081,y              ; $f4af: 99 81 00
	lda $e9,x                ; $f4b2: b5 e9
	lsr                      ; $f4b4: 4a
	lsr                      ; $f4b5: 4a
	lsr                      ; $f4b6: 4a
	lsr                      ; $f4b7: 4a
	bne $f4c0                ; $f4b8: d0 06
loc_f4ba:
	cpx #$02                 ; $f4ba: e0 02 WSYNC
	bne $f4c0                ; $f4bc: d0 02
loc_f4be:
	lda #$0a                 ; $f4be: a9 0a CTRLPF
loc_f4c0:
	sta $0085,y              ; $f4c0: 99 85 00
	dey                      ; $f4c3: 88
	dey                      ; $f4c4: 88
	bpl $f4aa                ; $f4c5: 10 e3
loc_f4c7:
	jsr $f4f8                ; $f4c7: 20 f8 f4
loc_f4ca:
	bne $f4a6                ; $f4ca: d0 da
loc_f4cc:
	ldx $9c                  ; $f4cc: a6 9c
	ldy #$3f                 ; $f4ce: a0 3f
	sty $0d                  ; $f4d0: 84 0d PF0
	sty $25,x                ; $f4d2: 94 25 VDELP0
	jsr $f54d                ; $f4d4: 20 4d f5
loc_f4d7:
	ldx $9b                  ; $f4d7: a6 9b
	lda $99                  ; $f4d9: a5 99
	jsr $f691                ; $f4db: 20 91 f6
loc_f4de:
	tya                      ; $f4de: 98
	eor $8f                  ; $f4df: 45 8f
	sta $0b,x                ; $f4e1: 95 0b REFP0
	lda #$97                 ; $f4e3: a9 97
	sta $8d                  ; $f4e5: 85 8d
	sta $1a                  ; $f4e7: 85 1a AUDV1
	lda $fb                  ; $f4e9: a5 fb
	sta $06,x                ; $f4eb: 95 06 COLUP0
	lda #$00                 ; $f4ed: a9 00 VSYNC
	sta $04,x                ; $f4ef: 95 04 NUSIZ0
	sta $2b                  ; $f4f1: 85 2b HMCLR
	sta $2c                  ; $f4f3: 85 2c CXCLR
	jmp $f018                ; $f4f5: 4c 18 f0

; === Code Block $f4f8-$f551 ===
.org $f4f8

loc_f4f8:
	stx $fa                  ; $f4f8: 86 fa
	dex                      ; $f4fa: ca
	stx $21                  ; $f4fb: 86 21 HMP1
	jsr $f69b                ; $f4fd: 20 9b f6
loc_f500:
	lda #$01                 ; $f500: a9 01 VBLANK
	ldy $83                  ; $f502: a4 83
	cpy #$0a                 ; $f504: c0 0a CTRLPF
	ldx $e8                  ; $f506: a6 e8
	ldy #$00                 ; $f508: a0 00 VSYNC
	bcs $f50f                ; $f50a: b0 03
loc_f50c:
	dey                      ; $f50c: 88
	stx $89                  ; $f50d: 86 89
loc_f50f:
	sty $f4                  ; $f50f: 84 f4
	sec                      ; $f511: 38
	sta $05                  ; $f512: 85 05 NUSIZ1
	rol                      ; $f514: 2a
	sta $04                  ; $f515: 85 04 NUSIZ0
	lda $f9                  ; $f517: a5 f9
	sta $06                  ; $f519: 85 06 COLUP0
	sta $07                  ; $f51b: 85 07 COLUP1
	lda #$70                 ; $f51d: a9 70
	sbc #$0f                 ; $f51f: e9 0f PF2
	tay                      ; $f521: a8
	lda $f70b,y              ; $f522: b9 0b f7
	and $f4                  ; $f525: 25 f4
	asl                      ; $f527: 0a
	sta $02                  ; $f528: 85 02 WSYNC
	ora ($85),y              ; $f52a: 11 85
	sta $1c                  ; $f52c: 85 1c GRP1
	lda ($89),y              ; $f52e: b1 89
	sta $1b                  ; $f530: 85 1b GRP0
	lda ($83),y              ; $f532: b1 83
	tax                      ; $f534: aa
	txs                      ; $f535: 9a
	lda ($81),y              ; $f536: b1 81
	tax                      ; $f538: aa
	lda #$00                 ; $f539: a9 00 VSYNC
	ror                      ; $f53b: 6a
	ora ($87),y              ; $f53c: 11 87
	stx $1b                  ; $f53e: 86 1b GRP0
	sta $1c                  ; $f540: 85 1c GRP1
	tsx                      ; $f542: ba
	stx $1b                  ; $f543: 86 1b GRP0
	tya                      ; $f545: 98
	bne $f51f                ; $f546: d0 d7
loc_f548:
	ldx #$fd                 ; $f548: a2 fd
	txs                      ; $f54a: 9a
	ldx $fa                  ; $f54b: a6 fa
loc_f54d:
	sta $1c                  ; $f54d: 85 1c GRP1
	sta $1b                  ; $f54f: 85 1b GRP0
	rts                      ; $f551: 60

; === Code Block $f552-$f55d ===
.org $f552

loc_f552:
	lda $8f                  ; $f552: a5 8f
	and #$0f                 ; $f554: 29 0f PF2
	cmp #$08                 ; $f556: c9 08 COLUPF
	bcc $f55c                ; $f558: 90 02
loc_f55a:
	eor #$0f                 ; $f55a: 49 0f PF2
loc_f55c:
	tay                      ; $f55c: a8
	rts                      ; $f55d: 60

; === Code Block $f55e-$f574 ===
.org $f55e

loc_f55e:
	sed                      ; $f55e: f8
	adc $e9                  ; $f55f: 65 e9
	cld                      ; $f561: d8
	bcc $f566                ; $f562: 90 02
loc_f564:
	adc #$9f                 ; $f564: 69 9f
loc_f566:
	cmp #$60                 ; $f566: c9 60 HMP0
	bcc $f56e                ; $f568: 90 04
loc_f56a:
	adc #$9f                 ; $f56a: 69 9f
	inc $e8                  ; $f56c: e6 e8
loc_f56e:
	sta $e9                  ; $f56e: 85 e9
	lda $e8                  ; $f570: a5 e8
	cmp #$05                 ; $f572: c9 05 NUSIZ1
	rts                      ; $f574: 60

; === Code Block $f575-$f59b ===
.org $f575

loc_f575:
	bit $92                  ; $f575: 24 92
	bpl $f59b                ; $f577: 10 22
loc_f579:
	lda $8f                  ; $f579: a5 8f
	cmp #$08                 ; $f57b: c9 08 COLUPF
	lda #$00                 ; $f57d: a9 00 VSYNC
	sta $16                  ; $f57f: 85 16 AUDC1
	sta $e1                  ; $f581: 85 e1
	sta $8e                  ; $f583: 85 8e
	adc #$47                 ; $f585: 69 47 COLUP1
	sta $8f                  ; $f587: 85 8f
	sta $e2                  ; $f589: 85 e2
	lda #$02                 ; $f58b: a9 02 WSYNC
loc_f58d:
	ldy #$74                 ; $f58d: a0 74
	sty $92                  ; $f58f: 84 92
	ldy #$e0                 ; $f591: a0 e0
loc_f593:
	bit $e1                  ; $f593: 24 e1
	bmi $f59b                ; $f595: 30 04
loc_f597:
	sta $15                  ; $f597: 85 15 AUDC0
	sty $e1                  ; $f599: 84 e1
loc_f59b:
	rts                      ; $f59b: 60

; === Code Block $f59c-$f5c1 ===
.org $f59c

loc_f59c:
	ldy $90                  ; $f59c: a4 90
	dey                      ; $f59e: 88
	tya                      ; $f59f: 98
	and #$07                 ; $f5a0: 29 07 COLUP1
	sta $90                  ; $f5a2: 85 90
	cmp #$06                 ; $f5a4: c9 06 COLUP0
	bne $f5c2                ; $f5a6: d0 1a
loc_f5a8:
	lda $f6                  ; $f5a8: a5 f6
	sed                      ; $f5aa: f8
	sbc #$02                 ; $f5ab: e9 02 WSYNC
	cld                      ; $f5ad: d8
	sta $f6                  ; $f5ae: 85 f6
	bcs $f5c2                ; $f5b0: b0 10
loc_f5b2:
	ldx #$04                 ; $f5b2: a2 04 NUSIZ0
	lda $eb                  ; $f5b4: a5 eb
	clc                      ; $f5b6: 18
	jsr $f55e                ; $f5b7: 20 5e f5
loc_f5ba:
	dex                      ; $f5ba: ca
	bpl $f5b4                ; $f5bb: 10 f7
loc_f5bd:
	stx $91                  ; $f5bd: 86 91
	stx $de                  ; $f5bf: 86 de
	rts                      ; $f5c1: 60

; === Code Block $f5c2-$f655 ===
.org $f5c2

loc_f5c2:
	ldx #$00                 ; $f5c2: a2 00 VSYNC
	lda $9f,x                ; $f5c4: b5 9f
	sta $9e,x                ; $f5c6: 95 9e
	inx                      ; $f5c8: e8
	cpx #$40                 ; $f5c9: e0 40 VSYNC
	bcc $f5c4                ; $f5cb: 90 f7
loc_f5cd:
	lda $90                  ; $f5cd: a5 90
	tay                      ; $f5cf: a8
	ora #$04                 ; $f5d0: 09 04 NUSIZ0
	tax                      ; $f5d2: aa
	and #$03                 ; $f5d3: 29 03 RSYNC
	bne $f5fd                ; $f5d5: d0 26
loc_f5d7:
	tay                      ; $f5d7: a8
	lda $f6                  ; $f5d8: a5 f6
	bne $f5e4                ; $f5da: d0 08
loc_f5dc:
	ldy #$04                 ; $f5dc: a0 04 NUSIZ0
	lda $90                  ; $f5de: a5 90
	bne $f5e4                ; $f5e0: d0 02
loc_f5e2:
	inx                      ; $f5e2: e8
	iny                      ; $f5e3: c8
loc_f5e4:
	lda $8c                  ; $f5e4: a5 8c
	cmp #$05                 ; $f5e6: c9 05 NUSIZ1
	bcc $f5fd                ; $f5e8: 90 13
loc_f5ea:
	sed                      ; $f5ea: f8
	lda $eb                  ; $f5eb: a5 eb
	sbc #$01                 ; $f5ed: e9 01 VBLANK
	sta $eb                  ; $f5ef: 85 eb
	cld                      ; $f5f1: d8
	tax                      ; $f5f2: aa
	bne $f5f9                ; $f5f3: d0 04
loc_f5f5:
	dex                      ; $f5f5: ca
	jsr $f5bd                ; $f5f6: 20 bd f5
loc_f5f9:
	lda #$07                 ; $f5f9: a9 07 COLUP1
	tax                      ; $f5fb: aa
	tay                      ; $f5fc: a8
loc_f5fd:
	lda $f7cd,x              ; $f5fd: bd cd f7
	sta $bd                  ; $f600: 85 bd
	sty $d5                  ; $f602: 84 d5
	lda $f7c9,x              ; $f604: bd c9 f7
	sta $b5                  ; $f607: 85 b5
	lda $f7d1,x              ; $f609: bd d1 f7
	sta $cd                  ; $f60c: 85 cd
	lda $f0                  ; $f60e: a5 f0
	asl                      ; $f610: 0a
	eor $f0                  ; $f611: 45 f0
	asl                      ; $f613: 0a
	asl                      ; $f614: 0a
	rol $f1                  ; $f615: 26 f1
	rol $f0                  ; $f617: 26 f0
	lda $f1                  ; $f619: a5 f1
	and #$3f                 ; $f61b: 29 3f
	sec                      ; $f61d: 38
	sbc #$20                 ; $f61e: e9 20 HMP0
	ldy $8c                  ; $f620: a4 8c
	cpy #$05                 ; $f622: c0 05 NUSIZ1
	bcs $f64e                ; $f624: b0 28
loc_f626:
	adc $f7de,x              ; $f626: 7d de f7
	bit $f8                  ; $f629: 24 f8
	bmi $f633                ; $f62b: 30 06
loc_f62d:
	clc                      ; $f62d: 18
	adc $f6f8,x              ; $f62e: 7d f8 f6
	bit $f8                  ; $f631: 24 f8
loc_f633:
	bvs $f64f                ; $f633: 70 1a
loc_f635:
	cpx #$04                 ; $f635: e0 04 NUSIZ0
	bne $f64f                ; $f637: d0 16
loc_f639:
	clc                      ; $f639: 18
	adc $c1                  ; $f63a: 65 c1
	cmp #$ec                 ; $f63c: c9 ec
	bcs $f644                ; $f63e: b0 04
loc_f640:
	cmp #$10                 ; $f640: c9 10 RESP0
	bcs $f646                ; $f642: b0 02
loc_f644:
	adc #$28                 ; $f644: 69 28 RESMP0
loc_f646:
	cmp #$70                 ; $f646: c9 70
	bcc $f653                ; $f648: 90 09
loc_f64a:
	sbc #$28                 ; $f64a: e9 28 RESMP0
	bcs $f646                ; $f64c: b0 f8
loc_f64e:
	asl                      ; $f64e: 0a
loc_f64f:
	dex                      ; $f64f: ca
	jsr $f656                ; $f650: 20 56 f6
loc_f653:
	sta $c5                  ; $f653: 85 c5
	rts                      ; $f655: 60

; === Code Block $f656-$f66d ===
.org $f656

loc_f656:
	clc                      ; $f656: 18
loc_f657:
	sta $f3                  ; $f657: 85 f3
	adc $be,x                ; $f659: 75 be
	bit $f3                  ; $f65b: 24 f3
	bmi $f663                ; $f65d: 30 04
loc_f65f:
	bcc $f667                ; $f65f: 90 06
loc_f661:
	bcs $f66b                ; $f661: b0 08
loc_f663:
	bcs $f66d                ; $f663: b0 08
loc_f665:
	adc #$a0                 ; $f665: 69 a0
loc_f667:
	cmp #$a0                 ; $f667: c9 a0
	bcc $f66d                ; $f669: 90 02
loc_f66b:
	sbc #$a0                 ; $f66b: e9 a0
loc_f66d:
	rts                      ; $f66d: 60

; === Code Block $f66e-$f690 ===
.org $f66e

loc_f66e:
	clc                      ; $f66e: 18
	adc #$02                 ; $f66f: 69 02 WSYNC
	tay                      ; $f671: a8
	and #$0f                 ; $f672: 29 0f PF2
	sta $f4                  ; $f674: 85 f4
	tya                      ; $f676: 98
	lsr                      ; $f677: 4a
	lsr                      ; $f678: 4a
	lsr                      ; $f679: 4a
	lsr                      ; $f67a: 4a
	tay                      ; $f67b: a8
	clc                      ; $f67c: 18
	adc $f4                  ; $f67d: 65 f4
	cmp #$0f                 ; $f67f: c9 0f PF2
	bcc $f686                ; $f681: 90 03
loc_f683:
	sbc #$0f                 ; $f683: e9 0f PF2
	iny                      ; $f685: c8
loc_f686:
	eor #$07                 ; $f686: 49 07 COLUP1
	asl                      ; $f688: 0a
	asl                      ; $f689: 0a
	asl                      ; $f68a: 0a
	sta $02                  ; $f68b: 85 02 WSYNC
	asl                      ; $f68d: 0a
	sta $2b                  ; $f68e: 85 2b HMCLR
	rts                      ; $f690: 60

; === Code Block $f691-$f69f ===
.org $f691

loc_f691:
	jsr $f66e                ; $f691: 20 6e f6
loc_f694:
	sta $20,x                ; $f694: 95 20 HMP0
	dey                      ; $f696: 88
	bpl $f696                ; $f697: 10 fd
loc_f699:
	sta $10,x                ; $f699: 95 10 RESP0
loc_f69b:
	sta $02                  ; $f69b: 85 02 WSYNC
	sta $2a                  ; $f69d: 85 2a HMOVE
	rts                      ; $f69f: 60

; === Vectors ===
.org $fffc
	.word reset
	.word reset
