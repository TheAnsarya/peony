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
RESP1            = $0011
RESM0            = $0012
AUDC0            = $0015
AUDC1            = $0016
AUDF0            = $0017
AUDF1            = $0018
AUDV0            = $0019
GRP0             = $001b
GRP1             = $001c
ENAM1            = $001e
ENABL            = $001f
HMP0             = $0020
HMP1             = $0021
VDELP0           = $0025
VDELP1           = $0026
RESMP0           = $0028
HMOVE            = $002a
HMCLR            = $002b
CXCLR            = $002c
VSYNC            = $0040
RESBL            = $0054
AUDC0            = $0055
CXCLR            = $006c
SWCHA            = $0280
SWCHB            = $0282
INTIM            = $0284
TIM64T           = $0296

; === Code Block $f000-$f00b ===
.org $f000

reset:
	sei                      ; $f000: 78
	cld                      ; $f001: d8
	ldx #$00                 ; $f002: a2 00 VSYNC
	txa                      ; $f004: 8a
	sta $00,x                ; $f005: 95 00 VSYNC
	txs                      ; $f007: 9a
	inx                      ; $f008: e8
	bne $f005                ; $f009: d0 fa
loc_f00b:
	jmp $f3c1                ; $f00b: 4c c1 f3

; === Code Block $f00e-$f100 ===
.org $f00e

loc_f00e:
	ldx #$12                 ; $f00e: a2 12 RESM0
	lda $f672,x              ; $f010: bd 72 f6
	ldy $ae                  ; $f013: a4 ae
	cpy #$20                 ; $f015: c0 20 HMP0
	bcc $f023                ; $f017: 90 0a
loc_f019:
	ldy $ac                  ; $f019: a4 ac
	bne $f025                ; $f01b: d0 08
loc_f01d:
	cpx #$04                 ; $f01d: e0 04 NUSIZ0
	bcc $f025                ; $f01f: 90 04
loc_f021:
	lda $82                  ; $f021: a5 82
loc_f023:
	eor $ae                  ; $f023: 45 ae
loc_f025:
	eor $83                  ; $f025: 45 83
	and $84                  ; $f027: 25 84
	sta $85,x                ; $f029: 95 85
	dex                      ; $f02b: ca
	bpl $f010                ; $f02c: 10 e2
loc_f02e:
	sta $09                  ; $f02e: 85 09 COLUBK
	ldx $a0                  ; $f030: a6 a0
	lda $87,x                ; $f032: b5 87
	sta $06                  ; $f034: 85 06 COLUP0
	sta $07                  ; $f036: 85 07 COLUP1
	lda $0284                ; $f038: ad 84 02 INTIM
	bne $f038                ; $f03b: d0 fb
loc_f03d:
	sta $02                  ; $f03d: 85 02 WSYNC
	sta $01                  ; $f03f: 85 01 VBLANK
	sta $08                  ; $f041: 85 08 COLUPF
	lda #$35                 ; $f043: a9 35
	sta $0a                  ; $f045: 85 0a CTRLPF
	sta $0d                  ; $f047: 85 0d PF0
	ldy #$03                 ; $f049: a0 03 RSYNC
	sty $04                  ; $f04b: 84 04 NUSIZ0
	sty $05                  ; $f04d: 84 05 NUSIZ1
	ldy #$07                 ; $f04f: a0 07 COLUP1
	sty $25                  ; $f051: 84 25 VDELP0
	sty $26                  ; $f053: 84 26 VDELP1
	sty $87                  ; $f055: 84 87
	lda ($fe),y              ; $f057: b1 fe
	sta $88                  ; $f059: 85 88
	sta $02                  ; $f05b: 85 02 WSYNC
	lda ($f4),y              ; $f05d: b1 f4
	sta $1b                  ; $f05f: 85 1b GRP0
	lda ($f6),y              ; $f061: b1 f6
	sta $1c                  ; $f063: 85 1c GRP1
	lda ($f8),y              ; $f065: b1 f8
	sta $1b                  ; $f067: 85 1b GRP0
	lda ($fc),y              ; $f069: b1 fc
	tax                      ; $f06b: aa
	lda ($fa),y              ; $f06c: b1 fa
	ldy $88                  ; $f06e: a4 88
	sta $1c                  ; $f070: 85 1c GRP1
	stx $1b                  ; $f072: 86 1b GRP0
	sty $1c                  ; $f074: 84 1c GRP1
	sta $1b                  ; $f076: 85 1b GRP0
	ldy $87                  ; $f078: a4 87
	dey                      ; $f07a: 88
	bpl $f055                ; $f07b: 10 d8
loc_f07d:
	iny                      ; $f07d: c8
	lda $bb                  ; $f07e: a5 bb
	sta $fa                  ; $f080: 85 fa
	lda $b2                  ; $f082: a5 b2
	sta $02                  ; $f084: 85 02 WSYNC
	sty $25                  ; $f086: 84 25 VDELP0
	sty $26                  ; $f088: 84 26 VDELP1
	sty $1b                  ; $f08a: 84 1b GRP0
	sty $1c                  ; $f08c: 84 1c GRP1
	sta $0b                  ; $f08e: 85 0b REFP0
	sta $20                  ; $f090: 85 20 HMP0
	and #$07                 ; $f092: 29 07 COLUP1
	tax                      ; $f094: aa
	dex                      ; $f095: ca
	bpl $f095                ; $f096: 10 fd
loc_f098:
	sta $10                  ; $f098: 85 10 RESP0
	sta $02                  ; $f09a: 85 02 WSYNC
	sty $05                  ; $f09c: 84 05 NUSIZ1
	sty $f9                  ; $f09e: 84 f9
	sty $04                  ; $f0a0: 84 04 NUSIZ0
	lda $99                  ; $f0a2: a5 99
	sta $21                  ; $f0a4: 85 21 HMP1
	and #$07                 ; $f0a6: 29 07 COLUP1
	tax                      ; $f0a8: aa
	lda $b1                  ; $f0a9: a5 b1
	dex                      ; $f0ab: ca
	bpl $f0ab                ; $f0ac: 10 fd
loc_f0ae:
	sta $11                  ; $f0ae: 85 11 RESP1
	sta $02                  ; $f0b0: 85 02 WSYNC
	sta $2a                  ; $f0b2: 85 2a HMOVE
	ldx #$1f                 ; $f0b4: a2 1f ENABL
	ora #$20                 ; $f0b6: 09 20 HMP0
	tay                      ; $f0b8: a8
	lda $a3                  ; $f0b9: a5 a3
	cmp #$01                 ; $f0bb: c9 01 VBLANK
	ror $88                  ; $f0bd: 66 88
	bmi $f0c9                ; $f0bf: 30 08
loc_f0c1:
	lda $ae                  ; $f0c1: a5 ae
	bne $f0ef                ; $f0c3: d0 2a
loc_f0c5:
	lda $a1                  ; $f0c5: a5 a1
	beq $f0ef                ; $f0c7: f0 26
loc_f0c9:
	lda $f652,x              ; $f0c9: bd 52 f6
	eor $83                  ; $f0cc: 45 83
	and $84                  ; $f0ce: 25 84
	sta $02                  ; $f0d0: 85 02 WSYNC
	sta $07                  ; $f0d2: 85 07 COLUP1
	lda $f6d3,x              ; $f0d4: bd d3 f6
	cpx #$16                 ; $f0d7: e0 16 AUDC1
	bne $f0dd                ; $f0d9: d0 02
loc_f0db:
	lda #$6c                 ; $f0db: a9 6c CXCLR
loc_f0dd:
	bcs $f0e7                ; $f0dd: b0 08
loc_f0df:
	lda #$6c                 ; $f0df: a9 6c CXCLR
	bit $88                  ; $f0e1: 24 88
	bmi $f0e7                ; $f0e3: 30 02
loc_f0e5:
	lda #$54                 ; $f0e5: a9 54 RESBL
loc_f0e7:
	sta $1c                  ; $f0e7: 85 1c GRP1
	dey                      ; $f0e9: 88
	dex                      ; $f0ea: ca
	cpx #$15                 ; $f0eb: e0 15 AUDC0
	bcs $f0c9                ; $f0ed: b0 da
loc_f0ef:
	lda $f652,x              ; $f0ef: bd 52 f6
	eor $83                  ; $f0f2: 45 83
	and $84                  ; $f0f4: 25 84
	cpx #$03                 ; $f0f6: e0 03 RSYNC
	bne $f103                ; $f0f8: d0 09
loc_f0fa:
	lda $86                  ; $f0fa: a5 86
	sta $02                  ; $f0fc: 85 02 WSYNC
	sta $09                  ; $f0fe: 85 09 COLUBK
	jmp $f10c                ; $f100: 4c 0c f1

; === Code Block $f103-$f233 ===
.org $f103

loc_f103:
	sta $02                  ; $f103: 85 02 WSYNC
	sta $07                  ; $f105: 85 07 COLUP1
	lda $f6d3,x              ; $f107: bd d3 f6
	sta $1c                  ; $f10a: 85 1c GRP1
loc_f10c:
	lda $0088,y              ; $f10c: b9 88 00
	sta $06                  ; $f10f: 85 06 COLUP0
	lda $f9                  ; $f111: a5 f9
	sta $1b                  ; $f113: 85 1b GRP0
	dey                      ; $f115: 88
	cpy #$10                 ; $f116: c0 10 RESP0
	bcs $f11e                ; $f118: b0 04
loc_f11a:
	lda ($fa),y              ; $f11a: b1 fa
	sta $f9                  ; $f11c: 85 f9
loc_f11e:
	dex                      ; $f11e: ca
	bpl $f0ef                ; $f11f: 10 ce
loc_f121:
	lda $0282                ; $f121: ad 82 02 SWCHB
	ldx $a0                  ; $f124: a6 a0
	bne $f129                ; $f126: d0 01
loc_f128:
	asl                      ; $f128: 0a
loc_f129:
	asl                      ; $f129: 0a
	lda #$00                 ; $f12a: a9 00 VSYNC
	sta $f6                  ; $f12c: 85 f6
	bcs $f132                ; $f12e: b0 02
loc_f130:
	lda #$05                 ; $f130: a9 05 NUSIZ1
loc_f132:
	sta $02                  ; $f132: 85 02 WSYNC
	sta $05                  ; $f134: 85 05 NUSIZ1
	lda $0088,y              ; $f136: b9 88 00
	sta $06                  ; $f139: 85 06 COLUP0
	lda $f9                  ; $f13b: a5 f9
	sta $1b                  ; $f13d: 85 1b GRP0
	lda #$00                 ; $f13f: a9 00 VSYNC
	sta $1c                  ; $f141: 85 1c GRP1
	sta $2c                  ; $f143: 85 2c CXCLR
	bcc $f149                ; $f145: 90 02
loc_f147:
	lda #$0a                 ; $f147: a9 0a CTRLPF
loc_f149:
	clc                      ; $f149: 18
	adc #$6c                 ; $f14a: 69 6c CXCLR
	sta $f5                  ; $f14c: 85 f5
	adc #$06                 ; $f14e: 69 06 COLUP0
	sta $f7                  ; $f150: 85 f7
	lda #$55                 ; $f152: a9 55 AUDC0
	sta $87                  ; $f154: 85 87
	dey                      ; $f156: 88
	bmi $f17a                ; $f157: 30 21
loc_f159:
	cpy #$10                 ; $f159: c0 10 RESP0
	bcc $f164                ; $f15b: 90 07
loc_f15d:
	sta $02                  ; $f15d: 85 02 WSYNC
	dec $87                  ; $f15f: c6 87
	dey                      ; $f161: 88
	bne $f159                ; $f162: d0 f5
loc_f164:
	lda ($fa),y              ; $f164: b1 fa
	sta $02                  ; $f166: 85 02 WSYNC
	sta $1b                  ; $f168: 85 1b GRP0
	lda $0088,y              ; $f16a: b9 88 00
	sta $06                  ; $f16d: 85 06 COLUP0
	dec $87                  ; $f16f: c6 87
	lda $08,x                ; $f171: b5 08 COLUPF
	bmi $f177                ; $f173: 30 02
loc_f175:
	dec $f7                  ; $f175: c6 f7
loc_f177:
	dey                      ; $f177: 88
	bpl $f164                ; $f178: 10 ea
loc_f17a:
	sta $02                  ; $f17a: 85 02 WSYNC
	inc $f6                  ; $f17c: e6 f6
	ldy $f6                  ; $f17e: a4 f6
	lda $00b2,y              ; $f180: b9 b2 00
	sta $20                  ; $f183: 85 20 HMP0
	sta $0b                  ; $f185: 85 0b REFP0
	and #$07                 ; $f187: 29 07 COLUP1
	tay                      ; $f189: a8
	dey                      ; $f18a: 88
	bpl $f18a                ; $f18b: 10 fd
loc_f18d:
	sta $10                  ; $f18d: 85 10 RESP0
	sta $02                  ; $f18f: 85 02 WSYNC
	ldy $f6                  ; $f191: a4 f6
	lda $00bb,y              ; $f193: b9 bb 00
	sta $fa                  ; $f196: 85 fa
	nop                      ; $f198: ea
	lda $98                  ; $f199: a5 98
	sta $21                  ; $f19b: 85 21 HMP1
	and #$07                 ; $f19d: 29 07 COLUP1
	tay                      ; $f19f: a8
	dey                      ; $f1a0: 88
	bpl $f1a0                ; $f1a1: 10 fd
loc_f1a3:
	sta $11                  ; $f1a3: 85 11 RESP1
	sta $02                  ; $f1a5: 85 02 WSYNC
	sta $2a                  ; $f1a7: 85 2a HMOVE
	ldy #$0f                 ; $f1a9: a0 0f PF2
	lda ($fa),y              ; $f1ab: b1 fa
	sta $1b                  ; $f1ad: 85 1b GRP0
	lda $0088,y              ; $f1af: b9 88 00
	sta $06                  ; $f1b2: 85 06 COLUP0
	lda $08,x                ; $f1b4: b5 08 COLUPF
	bmi $f1ba                ; $f1b6: 30 02
loc_f1b8:
	dec $f7                  ; $f1b8: c6 f7
loc_f1ba:
	dey                      ; $f1ba: 88
	sta $2b                  ; $f1bb: 85 2b HMCLR
	lda $f6                  ; $f1bd: a5 f6
	cmp $ac                  ; $f1bf: c5 ac
	bne $f1c8                ; $f1c1: d0 05
loc_f1c3:
	lda $82                  ; $f1c3: a5 82
	sta $0088,y              ; $f1c5: 99 88 00
loc_f1c8:
	lda ($fa),y              ; $f1c8: b1 fa
	sta $02                  ; $f1ca: 85 02 WSYNC
	sta $1b                  ; $f1cc: 85 1b GRP0
	lda $0088,y              ; $f1ce: b9 88 00
	sta $06                  ; $f1d1: 85 06 COLUP0
	dec $87                  ; $f1d3: c6 87
	beq $f209                ; $f1d5: f0 32
loc_f1d7:
	lda $08,x                ; $f1d7: b5 08 COLUPF
	bmi $f1dd                ; $f1d9: 30 02
loc_f1db:
	dec $f7                  ; $f1db: c6 f7
loc_f1dd:
	dey                      ; $f1dd: 88
	bpl $f1bd                ; $f1de: 10 dd
loc_f1e0:
	inc $f6                  ; $f1e0: e6 f6
	ldy $f6                  ; $f1e2: a4 f6
	lda $00bb,y              ; $f1e4: b9 bb 00
	sta $02                  ; $f1e7: 85 02 WSYNC
	nop                      ; $f1e9: ea
	sta $fa                  ; $f1ea: 85 fa
	sty $f8                  ; $f1ec: 84 f8
	lda $00b2,y              ; $f1ee: b9 b2 00
	sta $20                  ; $f1f1: 85 20 HMP0
	sta $0b                  ; $f1f3: 85 0b REFP0
	and #$07                 ; $f1f5: 29 07 COLUP1
	tay                      ; $f1f7: a8
	dey                      ; $f1f8: 88
	bpl $f1f8                ; $f1f9: 10 fd
loc_f1fb:
	sta $10                  ; $f1fb: 85 10 RESP0
	sta $02                  ; $f1fd: 85 02 WSYNC
	sta $2a                  ; $f1ff: 85 2a HMOVE
	ldy #$10                 ; $f201: a0 10 RESP0
	dec $87                  ; $f203: c6 87
	beq $f210                ; $f205: f0 09
loc_f207:
	bne $f1d3                ; $f207: d0 ca
loc_f209:
	ldx #$30                 ; $f209: a2 30
	dey                      ; $f20b: 88
	bmi $f254                ; $f20c: 30 46
loc_f20e:
	bpl $f21f                ; $f20e: 10 0f
loc_f210:
	dey                      ; $f210: 88
	ldx #$2f                 ; $f211: a2 2f
	stx $88                  ; $f213: 86 88
	ldx $a0                  ; $f215: a6 a0
	lda $08,x                ; $f217: b5 08 COLUPF
	bmi $f21d                ; $f219: 30 02
loc_f21b:
	dec $f7                  ; $f21b: c6 f7
loc_f21d:
	ldx $88                  ; $f21d: a6 88
loc_f21f:
	lda $f6                  ; $f21f: a5 f6
	cmp $ac                  ; $f221: c5 ac
	bne $f236                ; $f223: d0 11
loc_f225:
	lda $f684,x              ; $f225: bd 84 f6
	and $84                  ; $f228: 25 84
	sta $02                  ; $f22a: 85 02 WSYNC
	sta $07                  ; $f22c: 85 07 COLUP1
	lda $0082                ; $f22e: ad 82 00
	sta $06                  ; $f231: 85 06 COLUP0
	jmp $f246                ; $f233: 4c 46 f2

; === Code Block $f236-$f254 ===
.org $f236

loc_f236:
	lda $f684,x              ; $f236: bd 84 f6
	eor $83                  ; $f239: 45 83
	and $84                  ; $f23b: 25 84
	sta $02                  ; $f23d: 85 02 WSYNC
	sta $07                  ; $f23f: 85 07 COLUP1
	lda $0088,y              ; $f241: b9 88 00
	sta $06                  ; $f244: 85 06 COLUP0
loc_f246:
	lda $c3,x                ; $f246: b5 c3
	sta $1c                  ; $f248: 85 1c GRP1
	lda ($fa),y              ; $f24a: b1 fa
	sta $1b                  ; $f24c: 85 1b GRP0
	dex                      ; $f24e: ca
	beq $f29a                ; $f24f: f0 49
loc_f251:
	dey                      ; $f251: 88
loc_f252:
	bpl $f213                ; $f252: 10 bf

; === Code Block $f254-$f3c1 ===
.org $f254

loc_f254:
	inc $f6                  ; $f254: e6 f6
	ldy $f6                  ; $f256: a4 f6
	bit $07                  ; $f258: 24 07 COLUP1
	bmi $f25e                ; $f25a: 30 02
loc_f25c:
	sty $f8                  ; $f25c: 84 f8
loc_f25e:
	lda $00bb,y              ; $f25e: b9 bb 00
	sta $fa                  ; $f261: 85 fa
	lda $00b2,y              ; $f263: b9 b2 00
	sta $20                  ; $f266: 85 20 HMP0
	sta $0b                  ; $f268: 85 0b REFP0
	and #$07                 ; $f26a: 29 07 COLUP1
	sta $02                  ; $f26c: 85 02 WSYNC
	tay                      ; $f26e: a8
	lda $f684,x              ; $f26f: bd 84 f6
	eor $83                  ; $f272: 45 83
	and $84                  ; $f274: 25 84
	sta $07                  ; $f276: 85 07 COLUP1
	lda $c3,x                ; $f278: b5 c3
	sta $1c                  ; $f27a: 85 1c GRP1
	dey                      ; $f27c: 88
	bpl $f27c                ; $f27d: 10 fd
loc_f27f:
	sta $10                  ; $f27f: 85 10 RESP0
	lda $f683,x              ; $f281: bd 83 f6
	eor $83                  ; $f284: 45 83
	sta $02                  ; $f286: 85 02 WSYNC
	sta $2a                  ; $f288: 85 2a HMOVE
	dex                      ; $f28a: ca
	beq $f29c                ; $f28b: f0 0f
loc_f28d:
	and $84                  ; $f28d: 25 84
	sta $07                  ; $f28f: 85 07 COLUP1
	lda $c3,x                ; $f291: b5 c3
	sta $1c                  ; $f293: 85 1c GRP1
	ldy #$0f                 ; $f295: a0 0f PF2
	dex                      ; $f297: ca
	bne $f252                ; $f298: d0 b8
loc_f29a:
	sta $02                  ; $f29a: 85 02 WSYNC
loc_f29c:
	stx $1b                  ; $f29c: 86 1b GRP0
	stx $1c                  ; $f29e: 86 1c GRP1
	lda $83                  ; $f2a0: a5 83
	and $84                  ; $f2a2: 25 84
	sta $02                  ; $f2a4: 85 02 WSYNC
	sta $0009                ; $f2a6: 8d 09 00 COLUBK
	eor #$88                 ; $f2a9: 49 88
	and $84                  ; $f2ab: 25 84
	sta $06                  ; $f2ad: 85 06 COLUP0
	sta $07                  ; $f2af: 85 07 COLUP1
	stx $2b                  ; $f2b1: 86 2b HMCLR
	stx $0b                  ; $f2b3: 86 0b REFP0
	stx $0c                  ; $f2b5: 86 0c REFP1
	lda #$11                 ; $f2b7: a9 11 RESP1
	sta $04                  ; $f2b9: 85 04 NUSIZ0
	sta $05                  ; $f2bb: 85 05 NUSIZ1
	sta $21                  ; $f2bd: 85 21 HMP1
	sta $10                  ; $f2bf: 85 10 RESP0
	sta $11                  ; $f2c1: 85 11 RESP1
	ldx #$07                 ; $f2c3: a2 07 COLUP1
	sta $02                  ; $f2c5: 85 02 WSYNC
	sta $2a                  ; $f2c7: 85 2a HMOVE
	lda $f6b5,x              ; $f2c9: bd b5 f6
	sta $1b                  ; $f2cc: 85 1b GRP0
	lda $f6bd,x              ; $f2ce: bd bd f6
	sta $1c                  ; $f2d1: 85 1c GRP1
	jsr $f651                ; $f2d3: 20 51 f6
loc_f2d6:
	lda $f6cd,x              ; $f2d6: bd cd f6
	tay                      ; $f2d9: a8
	lda $f6c5,x              ; $f2da: bd c5 f6
	sta $1b                  ; $f2dd: 85 1b GRP0
	sty $1c                  ; $f2df: 84 1c GRP1
	sta $2b                  ; $f2e1: 85 2b HMCLR
	dex                      ; $f2e3: ca
	bpl $f2c5                ; $f2e4: 10 df
loc_f2e6:
	lda #$21                 ; $f2e6: a9 21 HMP1
	sta $0296                ; $f2e8: 8d 96 02 TIM64T
	ldx $0280                ; $f2eb: ae 80 02 SWCHA
	inx                      ; $f2ee: e8
	beq $f2f3                ; $f2ef: f0 02
loc_f2f1:
	sty $9e                  ; $f2f1: 84 9e
loc_f2f3:
	sec                      ; $f2f3: 38
	lda $f7                  ; $f2f4: a5 f7
	sbc #$05                 ; $f2f6: e9 05 NUSIZ1
	bpl $f2fb                ; $f2f8: 10 01
loc_f2fa:
	tya                      ; $f2fa: 98
loc_f2fb:
	sec                      ; $f2fb: 38
	sbc $9d                  ; $f2fc: e5 9d
	clc                      ; $f2fe: 18
	bpl $f302                ; $f2ff: 10 01
loc_f301:
	sec                      ; $f301: 38
loc_f302:
	ror                      ; $f302: 6a
	cmp #$02                 ; $f303: c9 02 WSYNC
	bcc $f30d                ; $f305: 90 06
loc_f307:
	cmp #$fe                 ; $f307: c9 fe
	bcs $f30d                ; $f309: b0 02
loc_f30b:
	sty $9e                  ; $f30b: 84 9e
loc_f30d:
	clc                      ; $f30d: 18
	adc $9d                  ; $f30e: 65 9d
	cmp $f5                  ; $f310: c5 f5
	bcc $f316                ; $f312: 90 02
loc_f314:
	lda $f5                  ; $f314: a5 f5
loc_f316:
	sta $9d                  ; $f316: 85 9d
	jsr $f63d                ; $f318: 20 3d f6
loc_f31b:
	sta $98                  ; $f31b: 85 98
	ldx #$0f                 ; $f31d: a2 0f PF2
	lda $f7ec,x              ; $f31f: bd ec f7
	sta $c4,x                ; $f322: 95 c4
	sta $d4,x                ; $f324: 95 d4
	sta $e4,x                ; $f326: 95 e4
	ldy $a1                  ; $f328: a4 a1
	beq $f334                ; $f32a: f0 08
loc_f32c:
	dey                      ; $f32c: 88
	beq $f336                ; $f32d: f0 07
loc_f32f:
	dey                      ; $f32f: 88
	beq $f338                ; $f330: f0 06
loc_f332:
	bne $f33a                ; $f332: d0 06
loc_f334:
	sty $e4,x                ; $f334: 94 e4
loc_f336:
	sty $d4,x                ; $f336: 94 d4
loc_f338:
	sty $c4,x                ; $f338: 94 c4
loc_f33a:
	dex                      ; $f33a: ca
	bpl $f31f                ; $f33b: 10 e2
loc_f33d:
	ldy $9c                  ; $f33d: a4 9c
	ldx $f7e8,y              ; $f33f: be e8 f7
	lda $af                  ; $f342: a5 af
	asl                      ; $f344: 0a
	and #$18                 ; $f345: 29 18 AUDF1
	tay                      ; $f347: a8
	lda #$07                 ; $f348: a9 07 COLUP1
	sta $f9                  ; $f34a: 85 f9
	lda $f708,y              ; $f34c: b9 08 f7
	sta $cc,x                ; $f34f: 95 cc
	iny                      ; $f351: c8
	inx                      ; $f352: e8
	dec $f9                  ; $f353: c6 f9
	bpl $f34c                ; $f355: 10 f5
loc_f357:
	lda $0284                ; $f357: ad 84 02 INTIM
	bne $f357                ; $f35a: d0 fb
loc_f35c:
	ldy #$82                 ; $f35c: a0 82
	sty $02                  ; $f35e: 84 02 WSYNC
	sty $01                  ; $f360: 84 01 VBLANK
	sty $00                  ; $f362: 84 00 VSYNC
	sty $02                  ; $f364: 84 02 WSYNC
	sty $02                  ; $f366: 84 02 WSYNC
	sty $02                  ; $f368: 84 02 WSYNC
	sta $00                  ; $f36a: 85 00 VSYNC
	inc $81                  ; $f36c: e6 81
	bne $f377                ; $f36e: d0 07
loc_f370:
	inc $9e                  ; $f370: e6 9e
	bne $f377                ; $f372: d0 03
loc_f374:
	sec                      ; $f374: 38
	ror $9e                  ; $f375: 66 9e
loc_f377:
	ldy #$ff                 ; $f377: a0 ff
	lda $0282                ; $f379: ad 82 02 SWCHB
	and #$08                 ; $f37c: 29 08 COLUPF
	bne $f382                ; $f37e: d0 02
loc_f380:
	ldy #$0f                 ; $f380: a0 0f PF2
loc_f382:
	tya                      ; $f382: 98
	ldy #$00                 ; $f383: a0 00 VSYNC
	bit $9e                  ; $f385: 24 9e
	bpl $f38d                ; $f387: 10 04
loc_f389:
	and #$f7                 ; $f389: 29 f7
	ldy $9e                  ; $f38b: a4 9e
loc_f38d:
	sty $83                  ; $f38d: 84 83
	asl $83                  ; $f38f: 06 83
	sta $84                  ; $f391: 85 84
	lda #$30                 ; $f393: a9 30
	sta $02                  ; $f395: 85 02 WSYNC
	sta $0296                ; $f397: 8d 96 02 TIM64T
	ldy #$00                 ; $f39a: a0 00 VSYNC
	sty $88                  ; $f39c: 84 88
	lda $0282                ; $f39e: ad 82 02 SWCHB
	lsr                      ; $f3a1: 4a
	bcs $f3b4                ; $f3a2: b0 10
loc_f3a4:
	jsr $f624                ; $f3a4: 20 24 f6
loc_f3a7:
	stx $ad                  ; $f3a7: 86 ad
	asl $80                  ; $f3a9: 06 80
	sec                      ; $f3ab: 38
	ror $80                  ; $f3ac: 66 80
	lda #$03                 ; $f3ae: a9 03 RSYNC
	sta $a1                  ; $f3b0: 85 a1
	sta $a6                  ; $f3b2: 85 a6
loc_f3b4:
	lsr                      ; $f3b4: 4a
	bcs $f3d0                ; $f3b5: b0 19
loc_f3b7:
	lda $9f                  ; $f3b7: a5 9f
	beq $f3bf                ; $f3b9: f0 04
loc_f3bb:
	dec $9f                  ; $f3bb: c6 9f
	bpl $f3d2                ; $f3bd: 10 13
loc_f3bf:
	inc $80                  ; $f3bf: e6 80

; === Code Block $f3c1-$f3fa ===
.org $f3c1

loc_f3c1:
	jsr $f624                ; $f3c1: 20 24 f6
loc_f3c4:
	lda $80                  ; $f3c4: a5 80
	and #$01                 ; $f3c6: 29 01 VBLANK
	sta $80                  ; $f3c8: 85 80
	tay                      ; $f3ca: a8
	iny                      ; $f3cb: c8
	sty $a5                  ; $f3cc: 84 a5
	ldy #$1e                 ; $f3ce: a0 1e ENAM1
	sty $9f                  ; $f3d0: 84 9f
	bit $80                  ; $f3d2: 24 80
	bpl $f3fa                ; $f3d4: 10 24
loc_f3d6:
	lda $a1                  ; $f3d6: a5 a1
	bne $f3e2                ; $f3d8: d0 08
loc_f3da:
	lda $81                  ; $f3da: a5 81
	and #$7f                 ; $f3dc: 29 7f
	bne $f3fa                ; $f3de: d0 1a
loc_f3e0:
	beq $f42a                ; $f3e0: f0 48
loc_f3e2:
	lda $ae                  ; $f3e2: a5 ae
	beq $f459                ; $f3e4: f0 73
loc_f3e6:
	cmp #$20                 ; $f3e6: c9 20 HMP0
	bcc $f41c                ; $f3e8: 90 32
loc_f3ea:
	beq $f3fd                ; $f3ea: f0 11
loc_f3ec:
	lda $ae                  ; $f3ec: a5 ae
	and #$0c                 ; $f3ee: 29 0c REFP1
	asl                      ; $f3f0: 0a
	asl                      ; $f3f1: 0a
	adc #$b8                 ; $f3f2: 69 b8
	ldx $ac                  ; $f3f4: a6 ac
	sta $bb,x                ; $f3f6: 95 bb
	dec $ae                  ; $f3f8: c6 ae
loc_f3fa:
	jmp $f59e                ; $f3fa: 4c 9e f5

; === Code Block $f3fd-$f41c ===
.org $f3fd

loc_f3fd:
	ldx $ac                  ; $f3fd: a6 ac
	lda #$00                 ; $f3ff: a9 00 VSYNC
	sta $bb,x                ; $f401: 95 bb
	lda #$2b                 ; $f403: a9 2b HMCLR
	sta $ae                  ; $f405: 85 ae
	ldx #$08                 ; $f407: a2 08 COLUPF
	lda $ab                  ; $f409: a5 ab
	beq $f40f                ; $f40b: f0 02
loc_f40d:
	dec $ab                  ; $f40d: c6 ab
loc_f40f:
	stx $ac                  ; $f40f: 86 ac
	lda $bb,x                ; $f411: b5 bb
	bne $f3ec                ; $f413: d0 d7
loc_f415:
	dex                      ; $f415: ca
	bpl $f40f                ; $f416: 10 f7
loc_f418:
	lda #$20                 ; $f418: a9 20 HMP0
	sta $ae                  ; $f41a: 85 ae

; === Code Block $f41c-$f42a ===
.org $f41c

loc_f41c:
	dec $ae                  ; $f41c: c6 ae
	bne $f3fa                ; $f41e: d0 da
loc_f420:
	lda $b0                  ; $f420: a5 b0
	bne $f426                ; $f422: d0 02
loc_f424:
	dec $a1                  ; $f424: c6 a1
loc_f426:
	lda $a6                  ; $f426: a5 a6
	beq $f442                ; $f428: f0 18

; === Code Block $f42a-$f46e ===
.org $f42a

loc_f42a:
	lda $80                  ; $f42a: a5 80
	lsr                      ; $f42c: 4a
	bcc $f442                ; $f42d: 90 13
loc_f42f:
	ldx #$04                 ; $f42f: a2 04 NUSIZ0
	ldy $a1,x                ; $f431: b4 a1
	lda $a6,x                ; $f433: b5 a6
	sta $a1,x                ; $f435: 95 a1
	sty $a6,x                ; $f437: 94 a6
	dex                      ; $f439: ca
	bpl $f431                ; $f43a: 10 f5
loc_f43c:
	lda $a0                  ; $f43c: a5 a0
	eor #$01                 ; $f43e: 49 01 VBLANK
	sta $a0                  ; $f440: 85 a0
loc_f442:
	ldx $a2                  ; $f442: a6 a2
	txa                      ; $f444: 8a
	beq $f451                ; $f445: f0 0a
loc_f447:
	dex                      ; $f447: ca
	stx $a2                  ; $f448: 86 a2
	lda $f6f3,x              ; $f44a: bd f3 f6
	lsr                      ; $f44d: 4a
	clc                      ; $f44e: 18
	adc #$01                 ; $f44f: 69 01 VBLANK
loc_f451:
	sta $ab                  ; $f451: 85 ab
	ldx #$ff                 ; $f453: a2 ff
	stx $ad                  ; $f455: 86 ad
	bne $f3fa                ; $f457: d0 a1
loc_f459:
	bit $ad                  ; $f459: 24 ad
	bpl $f471                ; $f45b: 10 14
loc_f45d:
	lda $0280                ; $f45d: ad 80 02 SWCHA
	ldx $a0                  ; $f460: a6 a0
	beq $f465                ; $f462: f0 01
loc_f464:
	asl                      ; $f464: 0a
loc_f465:
	asl                      ; $f465: 0a
	lda #$00                 ; $f466: a9 00 VSYNC
	bcs $f46c                ; $f468: b0 02
loc_f46a:
	sta $ad                  ; $f46a: 85 ad
loc_f46c:
	sta $b1                  ; $f46c: 85 b1
	jmp $f596                ; $f46e: 4c 96 f5

; === Code Block $f471-$f525 ===
.org $f471

loc_f471:
	lda $81                  ; $f471: a5 81
	and #$0f                 ; $f473: 29 0f PF2
	bne $f482                ; $f475: d0 0b
loc_f477:
	jsr $f62e                ; $f477: 20 2e f6
loc_f47a:
	bcs $f482                ; $f47a: b0 06
loc_f47c:
	lda $9b                  ; $f47c: a5 9b
	eor #$ff                 ; $f47e: 49 ff
	sta $9b                  ; $f480: 85 9b
loc_f482:
	bit $ad                  ; $f482: 24 ad
	bvs $f4b6                ; $f484: 70 30
loc_f486:
	lda $b1                  ; $f486: a5 b1
	cmp #$11                 ; $f488: c9 11 RESP1
	bcs $f4b6                ; $f48a: b0 2a
loc_f48c:
	cmp #$02                 ; $f48c: c9 02 WSYNC
	bcc $f4b6                ; $f48e: 90 26
loc_f490:
	lda $a2                  ; $f490: a5 a2
	bit $9b                  ; $f492: 24 9b
	bpl $f499                ; $f494: 10 03
loc_f496:
	eor #$ff                 ; $f496: 49 ff
	clc                      ; $f498: 18
loc_f499:
	adc $9a                  ; $f499: 65 9a
	cmp #$f0                 ; $f49b: c9 f0
	bcc $f4a5                ; $f49d: 90 06
loc_f49f:
	ldx #$00                 ; $f49f: a2 00 VSYNC
	lda #$05                 ; $f4a1: a9 05 NUSIZ1
	bne $f4ad                ; $f4a3: d0 08
loc_f4a5:
	cmp #$76                 ; $f4a5: c9 76
	bcc $f4af                ; $f4a7: 90 06
loc_f4a9:
	ldx #$ff                 ; $f4a9: a2 ff
	lda #$76                 ; $f4ab: a9 76
loc_f4ad:
	stx $9b                  ; $f4ad: 86 9b
loc_f4af:
	sta $9a                  ; $f4af: 85 9a
	jsr $f63d                ; $f4b1: 20 3d f6
loc_f4b4:
	sta $99                  ; $f4b4: 85 99
loc_f4b6:
	bit $07                  ; $f4b6: 24 07 COLUP1
	bpl $f512                ; $f4b8: 10 58
loc_f4ba:
	ldx $f8                  ; $f4ba: a6 f8
	lda #$00                 ; $f4bc: a9 00 VSYNC
	sta $bb,x                ; $f4be: 95 bb
	ldy #$02                 ; $f4c0: a0 02 WSYNC
	cpx #$06                 ; $f4c2: e0 06 COLUP0
	bcc $f4ca                ; $f4c4: 90 04
loc_f4c6:
	beq $f4c9                ; $f4c6: f0 01
loc_f4c8:
	dey                      ; $f4c8: 88
loc_f4c9:
	dey                      ; $f4c9: 88
loc_f4ca:
	ldx $a1                  ; $f4ca: a6 a1
	cpx #$02                 ; $f4cc: e0 02 WSYNC
	beq $f4d4                ; $f4ce: f0 04
loc_f4d0:
	bcs $f4d8                ; $f4d0: b0 06
loc_f4d2:
	ldy #$02                 ; $f4d2: a0 02 WSYNC
loc_f4d4:
	tya                      ; $f4d4: 98
	bne $f4d8                ; $f4d5: d0 01
loc_f4d7:
	iny                      ; $f4d7: c8
loc_f4d8:
	sty $9c                  ; $f4d8: 84 9c
	lda #$10                 ; $f4da: a9 10 RESP0
	sta $af                  ; $f4dc: 85 af
	sed                      ; $f4de: f8
	clc                      ; $f4df: 18
	lda $a2                  ; $f4e0: a5 a2
	adc #$01                 ; $f4e2: 69 01 VBLANK
	ldx #$02                 ; $f4e4: a2 02 WSYNC
	ldy $a4                  ; $f4e6: a4 a4
	adc $a3,x                ; $f4e8: 75 a3
	sta $a3,x                ; $f4ea: 95 a3
	lda #$00                 ; $f4ec: a9 00 VSYNC
	dex                      ; $f4ee: ca
	bpl $f4e8                ; $f4ef: 10 f7
loc_f4f1:
	cld                      ; $f4f1: d8
	bcc $f4fe                ; $f4f2: 90 0a
loc_f4f4:
	sta $a1                  ; $f4f4: 85 a1
	lda #$99                 ; $f4f6: a9 99
	sta $a3                  ; $f4f8: 85 a3
	sta $a4                  ; $f4fa: 85 a4
	sta $a5                  ; $f4fc: 85 a5
loc_f4fe:
	tya                      ; $f4fe: 98
	eor $a4                  ; $f4ff: 45 a4
	and #$f0                 ; $f501: 29 f0
	beq $f512                ; $f503: f0 0d
loc_f505:
	ldx $a1                  ; $f505: a6 a1
	inx                      ; $f507: e8
	cpx #$04                 ; $f508: e0 04 NUSIZ0
	bcs $f50e                ; $f50a: b0 02
loc_f50c:
	stx $a1                  ; $f50c: 86 a1
loc_f50e:
	lda #$3f                 ; $f50e: a9 3f
	sta $b0                  ; $f510: 85 b0
loc_f512:
	ldx $f6                  ; $f512: a6 f6
	lda $bb,x                ; $f514: b5 bb
	beq $f528                ; $f516: f0 10
loc_f518:
	ldx #$08                 ; $f518: a2 08 COLUPF
	lda $bb,x                ; $f51a: b5 bb
	beq $f522                ; $f51c: f0 04
loc_f51e:
	lda #$78                 ; $f51e: a9 78
	sta $bb,x                ; $f520: 95 bb
loc_f522:
	dex                      ; $f522: ca
	bpl $f51a                ; $f523: 10 f5
loc_f525:
	jmp $f403                ; $f525: 4c 03 f4

; === Code Block $f528-$f596 ===
.org $f528

loc_f528:
	ldx #$08                 ; $f528: a2 08 COLUPF
	lda $bb,x                ; $f52a: b5 bb
	beq $f53f                ; $f52c: f0 11
loc_f52e:
	dec $88                  ; $f52e: c6 88
	jsr $f62e                ; $f530: 20 2e f6
loc_f533:
	eor $81                  ; $f533: 45 81
	and #$03                 ; $f535: 29 03 RSYNC
	asl                      ; $f537: 0a
	asl                      ; $f538: 0a
	asl                      ; $f539: 0a
	asl                      ; $f53a: 0a
	adc #$78                 ; $f53b: 69 78
	sta $bb,x                ; $f53d: 95 bb
loc_f53f:
	dex                      ; $f53f: ca
	bpl $f52a                ; $f540: 10 e8
loc_f542:
	lda $a2                  ; $f542: a5 a2
	lsr                      ; $f544: 4a
	clc                      ; $f545: 18
	adc #$01                 ; $f546: 69 01 VBLANK
	adc $b1                  ; $f548: 65 b1
	sta $b1                  ; $f54a: 85 b1
	sec                      ; $f54c: 38
	sbc #$12                 ; $f54d: e9 12 RESM0
	bcc $f59e                ; $f54f: 90 4d
loc_f551:
	sta $b1                  ; $f551: 85 b1
	ldx #$07                 ; $f553: a2 07 COLUP1
	lda $b2,x                ; $f555: b5 b2
	sta $b3,x                ; $f557: 95 b3
	lda $bb,x                ; $f559: b5 bb
	sta $bc,x                ; $f55b: 95 bc
	dex                      ; $f55d: ca
	bpl $f555                ; $f55e: 10 f5
loc_f560:
	lda #$00                 ; $f560: a9 00 VSYNC
	sta $bb                  ; $f562: 85 bb
	ldx $a2                  ; $f564: a6 a2
	bit $ad                  ; $f566: 24 ad
	bvc $f579                ; $f568: 50 0f
loc_f56a:
	lda $88                  ; $f56a: a5 88
	ora $af                  ; $f56c: 05 af
	bne $f59e                ; $f56e: d0 2e
loc_f570:
	asl $ad                  ; $f570: 06 ad
	cpx #$07                 ; $f572: e0 07 COLUP1
	bcs $f579                ; $f574: b0 03
loc_f576:
	inx                      ; $f576: e8
	stx $a2                  ; $f577: 86 a2
loc_f579:
	txa                      ; $f579: 8a
	lsr                      ; $f57a: 4a
	bcs $f581                ; $f57b: b0 04
loc_f57d:
	lda $bc                  ; $f57d: a5 bc
	bne $f59e                ; $f57f: d0 1d
loc_f581:
	inc $ab                  ; $f581: e6 ab
	lda $ab                  ; $f583: a5 ab
	cmp $f6f3,x              ; $f585: dd f3 f6
	bcc $f592                ; $f588: 90 08
loc_f58a:
	lda #$00                 ; $f58a: a9 00 VSYNC
	sta $ab                  ; $f58c: 85 ab
	lda #$7f                 ; $f58e: a9 7f
	sta $ad                  ; $f590: 85 ad
loc_f592:
	lda $82                  ; $f592: a5 82
	and #$08                 ; $f594: 29 08 COLUPF

; === Code Block $f596-$f59e ===
.org $f596

loc_f596:
	ora $99                  ; $f596: 05 99
	sta $b2                  ; $f598: 85 b2
	lda #$78                 ; $f59a: a9 78
	sta $bb                  ; $f59c: 85 bb

; === Code Block $f59e-$f621 ===
.org $f59e

loc_f59e:
	jsr $f62e                ; $f59e: 20 2e f6
loc_f5a1:
	and #$03                 ; $f5a1: 29 03 RSYNC
	tax                      ; $f5a3: aa
	ldy #$00                 ; $f5a4: a0 00 VSYNC
	lda $88                  ; $f5a6: a5 88
	beq $f5b2                ; $f5a8: f0 08
loc_f5aa:
	txa                      ; $f5aa: 8a
	lsr                      ; $f5ab: 4a
	adc #$01                 ; $f5ac: 69 01 VBLANK
	sta $19                  ; $f5ae: 85 19 AUDV0
	ldy #$08                 ; $f5b0: a0 08 COLUPF
loc_f5b2:
	lda $af                  ; $f5b2: a5 af
	beq $f5c5                ; $f5b4: f0 0f
loc_f5b6:
	ldy #$08                 ; $f5b6: a0 08 COLUPF
	dec $af                  ; $f5b8: c6 af
	cmp #$0f                 ; $f5ba: c9 0f PF2
	bcc $f5c2                ; $f5bc: 90 04
loc_f5be:
	ldy #$0c                 ; $f5be: a0 0c REFP1
	sbc $a2                  ; $f5c0: e5 a2
loc_f5c2:
	tax                      ; $f5c2: aa
	sty $19                  ; $f5c3: 84 19 AUDV0
loc_f5c5:
	lda $ae                  ; $f5c5: a5 ae
	beq $f5d8                ; $f5c7: f0 0f
loc_f5c9:
	ldy #$08                 ; $f5c9: a0 08 COLUPF
	ldx #$08                 ; $f5cb: a2 08 COLUPF
	adc $ac                  ; $f5cd: 65 ac
	cmp #$20                 ; $f5cf: c9 20 HMP0
	bcs $f5d6                ; $f5d1: b0 03
loc_f5d3:
	lsr                      ; $f5d3: 4a
	ldx #$1f                 ; $f5d4: a2 1f ENABL
loc_f5d6:
	sta $19                  ; $f5d6: 85 19 AUDV0
loc_f5d8:
	lda $b0                  ; $f5d8: a5 b0
	beq $f5e8                ; $f5da: f0 0c
loc_f5dc:
	dec $b0                  ; $f5dc: c6 b0
	tax                      ; $f5de: aa
	lsr                      ; $f5df: 4a
	lsr                      ; $f5e0: 4a
	bcc $f5e4                ; $f5e1: 90 01
loc_f5e3:
	tax                      ; $f5e3: aa
loc_f5e4:
	ldy #$0c                 ; $f5e4: a0 0c REFP1
	sty $19                  ; $f5e6: 84 19 AUDV0
loc_f5e8:
	stx $17                  ; $f5e8: 86 17 AUDF0
	sty $15                  ; $f5ea: 84 15 AUDC0
	ldy #$02                 ; $f5ec: a0 02 WSYNC
	tya                      ; $f5ee: 98
	asl                      ; $f5ef: 0a
	asl                      ; $f5f0: 0a
	tax                      ; $f5f1: aa
	lda $00a3,y              ; $f5f2: b9 a3 00
	and #$f0                 ; $f5f5: 29 f0
	lsr                      ; $f5f7: 4a
	adc #$28                 ; $f5f8: 69 28 RESMP0
	sta $f4,x                ; $f5fa: 95 f4
	lda $00a3,y              ; $f5fc: b9 a3 00
	and #$0f                 ; $f5ff: 29 0f PF2
	asl                      ; $f601: 0a
	asl                      ; $f602: 0a
	asl                      ; $f603: 0a
	adc #$28                 ; $f604: 69 28 RESMP0
	sta $f6,x                ; $f606: 95 f6
	lda #$f7                 ; $f608: a9 f7
	sta $f5,x                ; $f60a: 95 f5
	sta $f7,x                ; $f60c: 95 f7
	dey                      ; $f60e: 88
	bpl $f5ee                ; $f60f: 10 dd
loc_f611:
	ldx #$00                 ; $f611: a2 00 VSYNC
	lda $f4,x                ; $f613: b5 f4
	eor #$28                 ; $f615: 49 28 RESMP0
	bne $f621                ; $f617: d0 08
loc_f619:
	sta $f4,x                ; $f619: 95 f4
	inx                      ; $f61b: e8
	inx                      ; $f61c: e8
	cpx #$0a                 ; $f61d: e0 0a CTRLPF
	bcc $f613                ; $f61f: 90 f2
loc_f621:
	jmp $f00e                ; $f621: 4c 0e f0

; === Code Block $f624-$f62d ===
.org $f624

loc_f624:
	lda #$00                 ; $f624: a9 00 VSYNC
	ldx #$25                 ; $f626: a2 25 VDELP0
	sta $9e,x                ; $f628: 95 9e
	dex                      ; $f62a: ca
	bpl $f628                ; $f62b: 10 fb
loc_f62d:
	rts                      ; $f62d: 60

; === Code Block $f62e-$f63c ===
.org $f62e

loc_f62e:
	lsr $82                  ; $f62e: 46 82
	rol                      ; $f630: 2a
	eor $82                  ; $f631: 45 82
	lsr                      ; $f633: 4a
	lda $82                  ; $f634: a5 82
	bcs $f63c                ; $f636: b0 04
loc_f638:
	ora #$40                 ; $f638: 09 40 VSYNC
	sta $82                  ; $f63a: 85 82
loc_f63c:
	rts                      ; $f63c: 60

; === Code Block $f63d-$f651 ===
.org $f63d

loc_f63d:
	ldy #$ff                 ; $f63d: a0 ff
	sec                      ; $f63f: 38
	iny                      ; $f640: c8
	sbc #$0f                 ; $f641: e9 0f PF2
	bcs $f640                ; $f643: b0 fb
loc_f645:
	sty $f9                  ; $f645: 84 f9
	eor #$ff                 ; $f647: 49 ff
	adc #$09                 ; $f649: 69 09 COLUBK
	asl                      ; $f64b: 0a
	asl                      ; $f64c: 0a
	asl                      ; $f64d: 0a
	asl                      ; $f64e: 0a
	ora $f9                  ; $f64f: 05 f9
	rts                      ; $f651: 60

; === Vectors ===
.org $fffc
	.word reset
	.word reset
