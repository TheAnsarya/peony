; 🌺 Peony Disassembly → 🌸 Poppy Assembly
; Platform: Atari 2600
; Size: 4096 bytes

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
PF1              = $000e
PF2              = $000f
RESP0            = $0010
RESP1            = $0011
RESM1            = $0013
RESBL            = $0014
AUDC0            = $0015
AUDC1            = $0016
AUDF0            = $0017
AUDV0            = $0019
GRP0             = $001b
GRP1             = $001c
ENABL            = $001f
HMP0             = $0020
HMP1             = $0021
HMM0             = $0022
HMBL             = $0024
VDELP0           = $0025
VDELP1           = $0026
RESMP1           = $0029
HMOVE            = $002a
HMCLR            = $002b
CXCLR            = $002c
VSYNC            = $0040
WSYNC            = $0042
NUSIZ0           = $0044
COLUP1           = $0047
REFP0            = $004b
REFP1            = $004c
RESP0            = $0050
RESM0            = $0052
RESM1            = $0053
RESBL            = $0054
AUDC1            = $0056
AUDF1            = $0058
AUDV0            = $0059
HMP0             = $0060
SWCHA            = $0280
SWCHB            = $0282
INTIM            = $0284
TIM64T           = $0296

; === Code Block $f000-$f307 ===
.org $f000

reset:
	sei                      ; $f000: 78
	cld                      ; $f001: d8
	ldx #$00                 ; $f002: a2 00 VSYNC
	lda #$00                 ; $f004: a9 00 VSYNC
	sta $00,x                ; $f006: 95 00 VSYNC
	txs                      ; $f008: 9a
	inx                      ; $f009: e8
	bne $f006                ; $f00a: d0 fa
loc_f00c:
	jsr $fa75                ; $f00c: 20 75 fa
loc_f00f:
	ldx #$08                 ; $f00f: a2 08 COLUPF
	lda $ffb8,x              ; $f011: bd b8 ff
	eor $87                  ; $f014: 45 87
	and $88                  ; $f016: 25 88
	sta $89,x                ; $f018: 95 89
	cpx #$04                 ; $f01a: e0 04 NUSIZ0
	bcs $f020                ; $f01c: b0 02
loc_f01e:
	sta $06,x                ; $f01e: 95 06 COLUP0
loc_f020:
	dex                      ; $f020: ca
	bpl $f011                ; $f021: 10 ee
loc_f023:
	ldy #$50                 ; $f023: a0 50 RESP0
	ldx #$90                 ; $f025: a2 90
	lda $e3                  ; $f027: a5 e3
	lsr                      ; $f029: 4a
	bcc $f02e                ; $f02a: 90 02
loc_f02c:
	ldx #$a0                 ; $f02c: a2 a0
loc_f02e:
	lda #$f0                 ; $f02e: a9 f0
	sta $bd                  ; $f030: 85 bd
	lda $9d                  ; $f032: a5 9d
	beq $f03e                ; $f034: f0 08
loc_f036:
	ldy #$60                 ; $f036: a0 60 HMP0
	ldx #$b0                 ; $f038: a2 b0
	stx $bd                  ; $f03a: 86 bd
	sty $bf                  ; $f03c: 84 bf
loc_f03e:
	stx $c1                  ; $f03e: 86 c1
	sty $c3                  ; $f040: 84 c3
	ldx $94                  ; $f042: a6 94
	lda $fc95,x              ; $f044: bd 95 fc
	bpl $f04d                ; $f047: 10 04
loc_f049:
	lda $8d                  ; $f049: a5 8d
	sta $91                  ; $f04b: 85 91
loc_f04d:
	ldy #$00                 ; $f04d: a0 00 VSYNC
	lda $ffe6,x              ; $f04f: bd e6 ff
	bpl $f07d                ; $f052: 10 29
loc_f054:
	lda $e9                  ; $f054: a5 e9
	cmp #$37                 ; $f056: c9 37
	bcs $f05e                ; $f058: b0 04
loc_f05a:
	cmp #$21                 ; $f05a: c9 21 HMP1
	bcs $f098                ; $f05c: b0 3a
loc_f05e:
	lda $9e                  ; $f05e: a5 9e
	bne $f098                ; $f060: d0 36
loc_f062:
	lda $d3                  ; $f062: a5 d3
	lsr                      ; $f064: 4a
	lsr                      ; $f065: 4a
	pha                      ; $f066: 48
	lsr                      ; $f067: 4a
	lsr                      ; $f068: 4a
	lsr                      ; $f069: 4a
	lsr                      ; $f06a: 4a
	tax                      ; $f06b: aa
	pla                      ; $f06c: 68
	and $fc92,x              ; $f06d: 3d 92 fc
	eor $fc90,x              ; $f070: 5d 90 fc
	pha                      ; $f073: 48
	tay                      ; $f074: a8
	lda $fc06,y              ; $f075: b9 06 fc
	tay                      ; $f078: a8
	pla                      ; $f079: 68
	clc                      ; $f07a: 18
	adc #$10                 ; $f07b: 69 10 RESP0
loc_f07d:
	clc                      ; $f07d: 18
	sty $f4                  ; $f07e: 84 f4
	adc #$06                 ; $f080: 69 06 COLUP0
	tay                      ; $f082: a8
	ldx #$06                 ; $f083: a2 06 COLUP0
	lda $9d                  ; $f085: a5 9d
	eor #$ff                 ; $f087: 49 ff
	sta $f6                  ; $f089: 85 f6
	lda $fbde,y              ; $f08b: b9 de fb
	sta $a0,x                ; $f08e: 95 a0
	ora $f6                  ; $f090: 05 f6
	sta $9f                  ; $f092: 85 9f
	dey                      ; $f094: 88
	dex                      ; $f095: ca
	bpl $f08b                ; $f096: 10 f3
loc_f098:
	ldx #$02                 ; $f098: a2 02 WSYNC
	lda #$00                 ; $f09a: a9 00 VSYNC
	ldy $d1                  ; $f09c: a4 d1
	bmi $f0a2                ; $f09e: 30 02
loc_f0a0:
	lda $e1,x                ; $f0a0: b5 e1
loc_f0a2:
	jsr $f388                ; $f0a2: 20 88 f3
loc_f0a5:
	sta $95,x                ; $f0a5: 95 95
	sty $98,x                ; $f0a7: 94 98
	stx $db                  ; $f0a9: 86 db
	dex                      ; $f0ab: ca
	bpl $f0a0                ; $f0ac: 10 f2
loc_f0ae:
	ldx $eb                  ; $f0ae: a6 eb
	lda $fbd6,x              ; $f0b0: bd d6 fb
	sta $9b                  ; $f0b3: 85 9b
	lda $fbda,x              ; $f0b5: bd da fb
	sta $9c                  ; $f0b8: 85 9c
	ldy #$0e                 ; $f0ba: a0 0e PF1
	ldx #$06                 ; $f0bc: a2 06 COLUP0
	lda ($bb),y              ; $f0be: b1 bb
	eor $87                  ; $f0c0: 45 87
	and $88                  ; $f0c2: 25 88
	sta $a7,x                ; $f0c4: 95 a7
	lda ($b7),y              ; $f0c6: b1 b7
	sta $ae,x                ; $f0c8: 95 ae
	dey                      ; $f0ca: 88
	dex                      ; $f0cb: ca
	bpl $f0be                ; $f0cc: 10 f0
loc_f0ce:
	lda $0284                ; $f0ce: ad 84 02 INTIM
	bne $f0ce                ; $f0d1: d0 fb
loc_f0d3:
	sta $02                  ; $f0d3: 85 02 WSYNC
	sta $2a                  ; $f0d5: 85 2a HMOVE
	sta $01                  ; $f0d7: 85 01 VBLANK
	sta $2c                  ; $f0d9: 85 2c CXCLR
	sta $f8                  ; $f0db: 85 f8
	jsr $f30a                ; $f0dd: 20 0a f3
loc_f0e0:
	ldx #$03                 ; $f0e0: a2 03 RSYNC
	ldy #$02                 ; $f0e2: a0 02 WSYNC
	jsr $f3b6                ; $f0e4: 20 b6 f3
loc_f0e7:
	inx                      ; $f0e7: e8
	ldy #$08                 ; $f0e8: a0 08 COLUPF
	jsr $f3b6                ; $f0ea: 20 b6 f3
loc_f0ed:
	lda $80                  ; $f0ed: a5 80
	sta $f8                  ; $f0ef: 85 f8
	lda #$58                 ; $f0f1: a9 58 AUDF1
	sta $c5                  ; $f0f3: 85 c5
	ldy $c7                  ; $f0f5: a4 c7
	bne $f0fb                ; $f0f7: d0 02
loc_f0f9:
	sta $c7                  ; $f0f9: 85 c7
loc_f0fb:
	lda #$50                 ; $f0fb: a9 50 RESP0
	sta $cb                  ; $f0fd: 85 cb
	jsr $f30a                ; $f0ff: 20 0a f3
loc_f102:
	sta $02                  ; $f102: 85 02 WSYNC
	sta $2a                  ; $f104: 85 2a HMOVE
	lda #$00                 ; $f106: a9 00 VSYNC
	sta $26                  ; $f108: 85 26 VDELP1
	sta $1b                  ; $f10a: 85 1b GRP0
	sta $1c                  ; $f10c: 85 1c GRP1
	lda $8e                  ; $f10e: a5 8e
	sta $06                  ; $f110: 85 06 COLUP0
	sta $07                  ; $f112: 85 07 COLUP1
	lda #$02                 ; $f114: a9 02 WSYNC
	sta $04                  ; $f116: 85 04 NUSIZ0
	sta $05                  ; $f118: 85 05 NUSIZ1
	lda $9b                  ; $f11a: a5 9b
	and #$0f                 ; $f11c: 29 0f PF2
	tax                      ; $f11e: aa
	lda $9c                  ; $f11f: a5 9c
	and #$0f                 ; $f121: 29 0f PF2
	tay                      ; $f123: a8
	sta $02                  ; $f124: 85 02 WSYNC
	sta $2a                  ; $f126: 85 2a HMOVE
	nop                      ; $f128: ea
	dex                      ; $f129: ca
	bpl $f129                ; $f12a: 10 fd
loc_f12c:
	sta $10                  ; $f12c: 85 10 RESP0
	lda $9b                  ; $f12e: a5 9b
	sta $20                  ; $f130: 85 20 HMP0
	lda $9c                  ; $f132: a5 9c
	sta $21                  ; $f134: 85 21 HMP1
	dey                      ; $f136: 88
	bpl $f136                ; $f137: 10 fd
loc_f139:
	sta $11                  ; $f139: 85 11 RESP1
	sta $02                  ; $f13b: 85 02 WSYNC
	sta $2a                  ; $f13d: 85 2a HMOVE
	lda #$05                 ; $f13f: a9 05 NUSIZ1
	sta $0a                  ; $f141: 85 0a CTRLPF
	ldy #$1f                 ; $f143: a0 1f ENABL
	lda $eb                  ; $f145: a5 eb
	asl                      ; $f147: 0a
	asl                      ; $f148: 0a
	tax                      ; $f149: aa
	clc                      ; $f14a: 18
	lda $db                  ; $f14b: a5 db
	adc $dc                  ; $f14d: 65 dc
	sta $db                  ; $f14f: 85 db
	sta $2b                  ; $f151: 85 2b HMCLR
	bcc $f159                ; $f153: 90 04
loc_f155:
	lda $dd                  ; $f155: a5 dd
	sta $24                  ; $f157: 85 24 HMBL
loc_f159:
	lda #$00                 ; $f159: a9 00 VSYNC
	cpy #$09                 ; $f15b: c0 09 COLUBK
	bcs $f164                ; $f15d: b0 05
loc_f15f:
	tya                      ; $f15f: 98
	lsr                      ; $f160: 4a
	lda $fbc5,y              ; $f161: b9 c5 fb
loc_f164:
	sta $02                  ; $f164: 85 02 WSYNC
	sta $2a                  ; $f166: 85 2a HMOVE
	sta $1b                  ; $f168: 85 1b GRP0
	sta $1c                  ; $f16a: 85 1c GRP1
	bcs $f178                ; $f16c: b0 0a
loc_f16e:
	lda $f3ef,x              ; $f16e: bd ef f3
	inx                      ; $f171: e8
	sta $0d                  ; $f172: 85 0d PF0
	sta $0e                  ; $f174: 85 0e PF1
	sta $0f                  ; $f176: 85 0f PF2
loc_f178:
	dey                      ; $f178: 88
	bne $f14a                ; $f179: d0 cf
loc_f17b:
	ldx $eb                  ; $f17b: a6 eb
	clc                      ; $f17d: 18
	lda $db                  ; $f17e: a5 db
	adc $dc                  ; $f180: 65 dc
	sta $db                  ; $f182: 85 db
	bcc $f188                ; $f184: 90 02
loc_f186:
	ldy $dd                  ; $f186: a4 dd
loc_f188:
	sty $24                  ; $f188: 84 24 HMBL
	lda #$01                 ; $f18a: a9 01 VBLANK
	sta $0a                  ; $f18c: 85 0a CTRLPF
	lda $fbce,x              ; $f18e: bd ce fb
	ldy $fbd2,x              ; $f191: bc d2 fb
	ldx $d4                  ; $f194: a6 d4
	sta $02                  ; $f196: 85 02 WSYNC
	sta $2a                  ; $f198: 85 2a HMOVE
	sta $0e                  ; $f19a: 85 0e PF1
	lda $8e                  ; $f19c: a5 8e
	sta $08                  ; $f19e: 85 08 COLUPF
	lda #$00                 ; $f1a0: a9 00 VSYNC
	sta $1b                  ; $f1a2: 85 1b GRP0
	sta $1c                  ; $f1a4: 85 1c GRP1
	sta $04                  ; $f1a6: 85 04 NUSIZ0
	sta $0d                  ; $f1a8: 85 0d PF0
	sty $0f                  ; $f1aa: 84 0f PF2
	stx $05                  ; $f1ac: 86 05 NUSIZ1
	ldx #$01                 ; $f1ae: a2 01 VBLANK
	clc                      ; $f1b0: 18
	lda $db                  ; $f1b1: a5 db
	adc $dc                  ; $f1b3: 65 dc
	sta $db                  ; $f1b5: 85 db
	lda #$00                 ; $f1b7: a9 00 VSYNC
	bcc $f1bd                ; $f1b9: 90 02
loc_f1bb:
	lda $dd                  ; $f1bb: a5 dd
loc_f1bd:
	sta $24                  ; $f1bd: 85 24 HMBL
	clc                      ; $f1bf: 18
	lda $db                  ; $f1c0: a5 db
	adc $dc                  ; $f1c2: 65 dc
	sta $db                  ; $f1c4: 85 db
	lda #$00                 ; $f1c6: a9 00 VSYNC
	bcc $f1cc                ; $f1c8: 90 02
loc_f1ca:
	lda $dd                  ; $f1ca: a5 dd
loc_f1cc:
	sta $02                  ; $f1cc: 85 02 WSYNC
	sta $2a                  ; $f1ce: 85 2a HMOVE
	ldy #$00                 ; $f1d0: a0 00 VSYNC
	sty $f6,x                ; $f1d2: 94 f6
	ldy $98,x                ; $f1d4: b4 98
	bne $f1e2                ; $f1d6: d0 0a
loc_f1d8:
	ldy #$60                 ; $f1d8: a0 60 HMP0
	sty $f6,x                ; $f1da: 94 f6
	sta $10,x                ; $f1dc: 95 10 RESP0
	sta $24                  ; $f1de: 85 24 HMBL
	bne $f1ea                ; $f1e0: d0 08
loc_f1e2:
	dey                      ; $f1e2: 88
	bne $f1e2                ; $f1e3: d0 fd
loc_f1e5:
	sta $0024                ; $f1e5: 8d 24 00 HMBL
	sta $10,x                ; $f1e8: 95 10 RESP0
loc_f1ea:
	sta $02                  ; $f1ea: 85 02 WSYNC
	sta $2a                  ; $f1ec: 85 2a HMOVE
	dex                      ; $f1ee: ca
	bpl $f1b0                ; $f1ef: 10 bf
loc_f1f1:
	jsr $f3a6                ; $f1f1: 20 a6 f3
loc_f1f4:
	lda $95                  ; $f1f4: a5 95
	sta $20                  ; $f1f6: 85 20 HMP0
	lda $96                  ; $f1f8: a5 96
	sta $21                  ; $f1fa: 85 21 HMP1
	sta $02                  ; $f1fc: 85 02 WSYNC
	sta $2a                  ; $f1fe: 85 2a HMOVE
	jsr $f3a6                ; $f200: 20 a6 f3
loc_f203:
	lda $f6                  ; $f203: a5 f6
	sta $20                  ; $f205: 85 20 HMP0
	lda $f7                  ; $f207: a5 f7
	sta $21                  ; $f209: 85 21 HMP1
	lda $e9                  ; $f20b: a5 e9
	clc                      ; $f20d: 18
	adc $f2                  ; $f20e: 65 f2
	adc #$15                 ; $f210: 69 15 AUDC0
	tay                      ; $f212: a8
	lda $e5                  ; $f213: a5 e5
	sta $0b                  ; $f215: 85 0b REFP0
	sta $02                  ; $f217: 85 02 WSYNC
	sta $2a                  ; $f219: 85 2a HMOVE
	sta $2c                  ; $f21b: 85 2c CXCLR
	ldx #$14                 ; $f21d: a2 14 RESBL
	stx $25                  ; $f21f: 86 25 VDELP0
	clc                      ; $f221: 18
	lda $db                  ; $f222: a5 db
	adc $dc                  ; $f224: 65 dc
	sta $db                  ; $f226: 85 db
	lda #$00                 ; $f228: a9 00 VSYNC
	sta $2b                  ; $f22a: 85 2b HMCLR
	bcc $f230                ; $f22c: 90 02
loc_f22e:
	lda $dd                  ; $f22e: a5 dd
loc_f230:
	sta $24                  ; $f230: 85 24 HMBL
	jsr $f372                ; $f232: 20 72 f3
loc_f235:
	sta $02                  ; $f235: 85 02 WSYNC
	sta $2a                  ; $f237: 85 2a HMOVE
	sta $06                  ; $f239: 85 06 COLUP0
	dex                      ; $f23b: ca
	bpl $f221                ; $f23c: 10 e3
loc_f23e:
	stx $25                  ; $f23e: 86 25 VDELP0
	inx                      ; $f240: e8
	stx $1c                  ; $f241: 86 1c GRP1
	beq $f24b                ; $f243: f0 06
loc_f245:
	lda #$00                 ; $f245: a9 00 VSYNC
	sta $1b                  ; $f247: 85 1b GRP0
	beq $f26b                ; $f249: f0 20
loc_f24b:
	ldx #$17                 ; $f24b: a2 17 AUDF0
	clc                      ; $f24d: 18
	lda $db                  ; $f24e: a5 db
	adc $dc                  ; $f250: 65 dc
	sta $db                  ; $f252: 85 db
	lda #$00                 ; $f254: a9 00 VSYNC
	bcc $f25a                ; $f256: 90 02
loc_f258:
	lda $dd                  ; $f258: a5 dd
loc_f25a:
	sta $24                  ; $f25a: 85 24 HMBL
	dey                      ; $f25c: 88
	cpy #$16                 ; $f25d: c0 16 AUDC1
	bcs $f245                ; $f25f: b0 e4
loc_f261:
	lda ($b5),y              ; $f261: b1 b5
	sta $1b                  ; $f263: 85 1b GRP0
	lda ($b9),y              ; $f265: b1 b9
	eor $87                  ; $f267: 45 87
	and $88                  ; $f269: 25 88
loc_f26b:
	sta $02                  ; $f26b: 85 02 WSYNC
	sta $2a                  ; $f26d: 85 2a HMOVE
	sta $06                  ; $f26f: 85 06 COLUP0
	lda #$00                 ; $f271: a9 00 VSYNC
	cpx $92                  ; $f273: e4 92
	bcs $f279                ; $f275: b0 02
loc_f277:
	sta $1f                  ; $f277: 85 1f ENABL
loc_f279:
	sta $1c                  ; $f279: 85 1c GRP1
	dex                      ; $f27b: ca
	bpl $f24d                ; $f27c: 10 cf
loc_f27e:
	jsr $f372                ; $f27e: 20 72 f3
loc_f281:
	ldx $02                  ; $f281: a6 02 WSYNC
	stx $85                  ; $f283: 86 85
	sta $02                  ; $f285: 85 02 WSYNC
	sta $2a                  ; $f287: 85 2a HMOVE
	sta $06                  ; $f289: 85 06 COLUP0
	lda $90                  ; $f28b: a5 90
	sta $08                  ; $f28d: 85 08 COLUPF
	ldx #$ff                 ; $f28f: a2 ff
	stx $0d                  ; $f291: 86 0d PF0
	stx $0e                  ; $f293: 86 0e PF1
	stx $0f                  ; $f295: 86 0f PF2
	lda $91                  ; $f297: a5 91
	sta $09                  ; $f299: 85 09 COLUBK
	inx                      ; $f29b: e8
	stx $1c                  ; $f29c: 86 1c GRP1
	ldx #$06                 ; $f29e: a2 06 COLUP0
	jsr $f372                ; $f2a0: 20 72 f3
loc_f2a3:
	sta $02                  ; $f2a3: 85 02 WSYNC
	sta $2a                  ; $f2a5: 85 2a HMOVE
	sta $06                  ; $f2a7: 85 06 COLUP0
	lda $a7,x                ; $f2a9: b5 a7
	sta $07                  ; $f2ab: 85 07 COLUP1
	lda $ae,x                ; $f2ad: b5 ae
	sta $1c                  ; $f2af: 85 1c GRP1
	lda $a0,x                ; $f2b1: b5 a0
	sta $0f                  ; $f2b3: 85 0f PF2
	dex                      ; $f2b5: ca
	bpl $f2a0                ; $f2b6: 10 e8
loc_f2b8:
	tya                      ; $f2b8: 98
	sec                      ; $f2b9: 38
	sbc #$08                 ; $f2ba: e9 08 COLUPF
	sta $f6                  ; $f2bc: 85 f6
	ldx #$00                 ; $f2be: a2 00 VSYNC
	ldy #$07                 ; $f2c0: a0 07 COLUP1
	lda #$00                 ; $f2c2: a9 00 VSYNC
	sta $1b                  ; $f2c4: 85 1b GRP0
	lda ($bb),y              ; $f2c6: b1 bb
	eor $87                  ; $f2c8: 45 87
	and $88                  ; $f2ca: 25 88
	sta $02                  ; $f2cc: 85 02 WSYNC
	sta $2a                  ; $f2ce: 85 2a HMOVE
	sta $07                  ; $f2d0: 85 07 COLUP1
	lda ($b7),y              ; $f2d2: b1 b7
	sta $1c                  ; $f2d4: 85 1c GRP1
	dey                      ; $f2d6: 88
	bmi $f2e0                ; $f2d7: 30 07
loc_f2d9:
	lda $a0,x                ; $f2d9: b5 a0
	sta $0f                  ; $f2db: 85 0f PF2
	inx                      ; $f2dd: e8
	bne $f2c2                ; $f2de: d0 e2
loc_f2e0:
	lda #$00                 ; $f2e0: a9 00 VSYNC
	sta $1b                  ; $f2e2: 85 1b GRP0
	ldx $9a                  ; $f2e4: a6 9a
	bne $f2ea                ; $f2e6: d0 02
loc_f2e8:
	lda #$60                 ; $f2e8: a9 60 HMP0
loc_f2ea:
	sta $f8                  ; $f2ea: 85 f8
	lda $07                  ; $f2ec: a5 07 COLUP1
	asl                      ; $f2ee: 0a
	lda $02                  ; $f2ef: a5 02 WSYNC
	ror                      ; $f2f1: 6a
	sta $86                  ; $f2f2: 85 86
	lda $e6                  ; $f2f4: a5 e6
	sta $0c                  ; $f2f6: 85 0c REFP1
	lda $9d                  ; $f2f8: a5 9d
	and #$04                 ; $f2fa: 29 04 NUSIZ0
	eor #$25                 ; $f2fc: 49 25 VDELP0
	tax                      ; $f2fe: aa
	ldy $8f                  ; $f2ff: a4 8f
	lda $8d                  ; $f301: a5 8d
	sta $02                  ; $f303: 85 02 WSYNC
	sta $2a                  ; $f305: 85 2a HMOVE
	jmp $f405                ; $f307: 4c 05 f4

; === Code Block $f30a-$f371 ===
.org $f30a

loc_f30a:
	sta $02                  ; $f30a: 85 02 WSYNC
	sta $2a                  ; $f30c: 85 2a HMOVE
	lda $89                  ; $f30e: a5 89
	sta $06                  ; $f310: 85 06 COLUP0
	sta $07                  ; $f312: 85 07 COLUP1
	ldy #$00                 ; $f314: a0 00 VSYNC
	sty $0b                  ; $f316: 84 0b REFP0
	sty $0c                  ; $f318: 84 0c REFP1
	ldx #$13                 ; $f31a: a2 13 RESM1
	stx $04                  ; $f31c: 86 04 NUSIZ0
	sta $10                  ; $f31e: 85 10 RESP0
	sta $11                  ; $f320: 85 11 RESP1
	stx $21                  ; $f322: 86 21 HMP1
	sta $02                  ; $f324: 85 02 WSYNC
	sta $2a                  ; $f326: 85 2a HMOVE
	stx $05                  ; $f328: 86 05 NUSIZ1
	iny                      ; $f32a: c8
	sty $0a                  ; $f32b: 84 0a CTRLPF
	lda #$07                 ; $f32d: a9 07 COLUP1
	sta $25                  ; $f32f: 85 25 VDELP0
	sta $26                  ; $f331: 85 26 VDELP1
	sta $f7                  ; $f333: 85 f7
	sta $2b                  ; $f335: 85 2b HMCLR
	jsr $f39f                ; $f337: 20 9f f3
loc_f33a:
	lda $f8                  ; $f33a: a5 f8
	ldy $f7                  ; $f33c: a4 f7
	lda ($cf),y              ; $f33e: b1 cf
	sta $f6                  ; $f340: 85 f6
	lda ($cd),y              ; $f342: b1 cd
	tax                      ; $f344: aa
	lda ($c5),y              ; $f345: b1 c5
	ora $f8                  ; $f347: 05 f8
	sta $2a                  ; $f349: 85 2a HMOVE
	sta $1b                  ; $f34b: 85 1b GRP0
	lda ($c7),y              ; $f34d: b1 c7
	sta $1c                  ; $f34f: 85 1c GRP1
	lda ($c9),y              ; $f351: b1 c9
	sta $1b                  ; $f353: 85 1b GRP0
	lda ($cb),y              ; $f355: b1 cb
	ldy $f6                  ; $f357: a4 f6
	sta $1c                  ; $f359: 85 1c GRP1
	stx $1b                  ; $f35b: 86 1b GRP0
	sty $1c                  ; $f35d: 84 1c GRP1
	sta $1b                  ; $f35f: 85 1b GRP0
	dec $f7                  ; $f361: c6 f7
	bpl $f33c                ; $f363: 10 d7
loc_f365:
	sta $02                  ; $f365: 85 02 WSYNC
	sta $2a                  ; $f367: 85 2a HMOVE
	lda #$00                 ; $f369: a9 00 VSYNC
	sta $1b                  ; $f36b: 85 1b GRP0
	sta $1c                  ; $f36d: 85 1c GRP1
	sta $1b                  ; $f36f: 85 1b GRP0
	rts                      ; $f371: 60

; === Code Block $f372-$f381 ===
.org $f372

loc_f372:
	dey                      ; $f372: 88
	cpy #$16                 ; $f373: c0 16 AUDC1
	bcs $f382                ; $f375: b0 0b
loc_f377:
	lda ($b5),y              ; $f377: b1 b5
	sta $1b                  ; $f379: 85 1b GRP0
	lda ($b9),y              ; $f37b: b1 b9
	eor $87                  ; $f37d: 45 87
	and $88                  ; $f37f: 25 88
	rts                      ; $f381: 60

; === Code Block $f382-$f388 ===
.org $f382

loc_f382:
	lda #$00                 ; $f382: a9 00 VSYNC
	sta $1b                  ; $f384: 85 1b GRP0
	beq $f381                ; $f386: f0 f9

; === Code Block $f388-$f3a5 ===
.org $f388

loc_f388:
	tay                      ; $f388: a8
	iny                      ; $f389: c8
	tya                      ; $f38a: 98
	and #$0f                 ; $f38b: 29 0f PF2
	sta $f6                  ; $f38d: 85 f6
	tya                      ; $f38f: 98
	lsr                      ; $f390: 4a
	lsr                      ; $f391: 4a
	lsr                      ; $f392: 4a
	lsr                      ; $f393: 4a
	tay                      ; $f394: a8
	clc                      ; $f395: 18
	adc $f6                  ; $f396: 65 f6
	cmp #$0f                 ; $f398: c9 0f PF2
	bcc $f39f                ; $f39a: 90 03
loc_f39c:
	sbc #$0f                 ; $f39c: e9 0f PF2
	iny                      ; $f39e: c8
loc_f39f:
	eor #$07                 ; $f39f: 49 07 COLUP1
	asl                      ; $f3a1: 0a
	asl                      ; $f3a2: 0a
	asl                      ; $f3a3: 0a
	asl                      ; $f3a4: 0a
	rts                      ; $f3a5: 60

; === Code Block $f3a6-$f3b5 ===
.org $f3a6

loc_f3a6:
	clc                      ; $f3a6: 18
	lda $db                  ; $f3a7: a5 db
	adc $dc                  ; $f3a9: 65 dc
	sta $db                  ; $f3ab: 85 db
	lda #$00                 ; $f3ad: a9 00 VSYNC
	bcc $f3b3                ; $f3af: 90 02
loc_f3b1:
	lda $dd                  ; $f3b1: a5 dd
loc_f3b3:
	sta $24                  ; $f3b3: 85 24 HMBL
	rts                      ; $f3b5: 60

; === Code Block $f3b6-$f3cc ===
.org $f3b6

loc_f3b6:
	lda $d5,x                ; $f3b6: b5 d5
	and #$f0                 ; $f3b8: 29 f0
	lsr                      ; $f3ba: 4a
	sta $00c5,y              ; $f3bb: 99 c5 00
	lda $d5,x                ; $f3be: b5 d5
	and #$0f                 ; $f3c0: 29 0f PF2
	asl                      ; $f3c2: 0a
	asl                      ; $f3c3: 0a
	asl                      ; $f3c4: 0a
	sta $00c7,y              ; $f3c5: 99 c7 00
	sta $02                  ; $f3c8: 85 02 WSYNC
	sta $2a                  ; $f3ca: 85 2a HMOVE
	rts                      ; $f3cc: 60

; === Code Block $f3cd-$f3d3 ===
.org $f3cd

loc_f3cd:
	lda #$07                 ; $f3cd: a9 07 COLUP1
	sta $16                  ; $f3cf: 85 16 AUDC1
	lda #$99                 ; $f3d1: a9 99

; === Code Block $f3d3-$f3ee ===
.org $f3d3

loc_f3d3:
	sed                      ; $f3d3: f8
	clc                      ; $f3d4: 18
	adc $d7                  ; $f3d5: 65 d7
	sta $d7                  ; $f3d7: 85 d7
	lda $d6                  ; $f3d9: a5 d6
	sbc #$00                 ; $f3db: e9 00 VSYNC
	sta $d6                  ; $f3dd: 85 d6
	lda $d5                  ; $f3df: a5 d5
	sbc #$00                 ; $f3e1: e9 00 VSYNC
	bcs $f3eb                ; $f3e3: b0 06
loc_f3e5:
	lda #$00                 ; $f3e5: a9 00 VSYNC
	sta $d6                  ; $f3e7: 85 d6
	sta $d7                  ; $f3e9: 85 d7
loc_f3eb:
	sta $d5                  ; $f3eb: 85 d5
	cld                      ; $f3ed: d8
	rts                      ; $f3ee: 60

; === Code Block $f3ff-$f405 ===
.org $f3ff

loc_f3ff:
	lda #$00                 ; $f3ff: a9 00 VSYNC
	sta $1b                  ; $f401: 85 1b GRP0
	beq $f42e                ; $f403: f0 29

; === Code Block $f405-$f6bc ===
.org $f405

loc_f405:
	sta $09                  ; $f405: 85 09 COLUBK
	sty $08                  ; $f407: 84 08 COLUPF
	lda #$00                 ; $f409: a9 00 VSYNC
	sta $1c                  ; $f40b: 85 1c GRP1
	lda #$ff                 ; $f40d: a9 ff
	sta $0e                  ; $f40f: 85 0e PF1
	lda $9f                  ; $f411: a5 9f
	sta $0f                  ; $f413: 85 0f PF2
	stx $0a                  ; $f415: 86 0a CTRLPF
	ldy $f6                  ; $f417: a4 f6
	lda #$90                 ; $f419: a9 90
	sta $0024                ; $f41b: 8d 24 00 HMBL
	cpy #$16                 ; $f41e: c0 16 AUDC1
	sta $14                  ; $f420: 85 14 RESBL
	bcs $f3ff                ; $f422: b0 db
loc_f424:
	lda ($b5),y              ; $f424: b1 b5
	sta $1b                  ; $f426: 85 1b GRP0
	lda ($b9),y              ; $f428: b1 b9
	eor $87                  ; $f42a: 45 87
	and $88                  ; $f42c: 25 88
	ldx $9a                  ; $f42e: a6 9a
	sta $02                  ; $f430: 85 02 WSYNC
	sta $2a                  ; $f432: 85 2a HMOVE
	sta $06                  ; $f434: 85 06 COLUP0
	beq $f438                ; $f436: f0 00
loc_f438:
	beq $f43a                ; $f438: f0 00
loc_f43a:
	lda #$00                 ; $f43a: a9 00 VSYNC
	sta $1c                  ; $f43c: 85 1c GRP1
	dex                      ; $f43e: ca
	bpl $f43e                ; $f43f: 10 fd
loc_f441:
	sta $0011                ; $f441: 8d 11 00 RESP1
	sta $2b                  ; $f444: 85 2b HMCLR
	sta $02                  ; $f446: 85 02 WSYNC
	sta $2a                  ; $f448: 85 2a HMOVE
	dey                      ; $f44a: 88
	cpy #$16                 ; $f44b: c0 16 AUDC1
	bcs $f45b                ; $f44d: b0 0c
loc_f44f:
	lda ($b9),y              ; $f44f: b1 b9
	eor $87                  ; $f451: 45 87
	and $88                  ; $f453: 25 88
	sta $06                  ; $f455: 85 06 COLUP0
	lda ($b5),y              ; $f457: b1 b5
	sta $1b                  ; $f459: 85 1b GRP0
loc_f45b:
	lda #$00                 ; $f45b: a9 00 VSYNC
	sta $1c                  ; $f45d: 85 1c GRP1
	lda $97                  ; $f45f: a5 97
	sta $21                  ; $f461: 85 21 HMP1
	ldx #$0b                 ; $f463: a2 0b REFP0
	dey                      ; $f465: 88
	dey                      ; $f466: 88
	cpy #$16                 ; $f467: c0 16 AUDC1
	bcs $f495                ; $f469: b0 2a
loc_f46b:
	lda ($b5),y              ; $f46b: b1 b5
	sta $1b                  ; $f46d: 85 1b GRP0
	lda ($b9),y              ; $f46f: b1 b9
	eor $87                  ; $f471: 45 87
	and $88                  ; $f473: 25 88
	sta $02                  ; $f475: 85 02 WSYNC
	sta $2a                  ; $f477: 85 2a HMOVE
	sta $06                  ; $f479: 85 06 COLUP0
	lda #$00                 ; $f47b: a9 00 VSYNC
	sta $1c                  ; $f47d: 85 1c GRP1
	lda $fc95,x              ; $f47f: bd 95 fc
	and $9d                  ; $f482: 25 9d
	sta $1f                  ; $f484: 85 1f ENABL
	dex                      ; $f486: ca
	bmi $f4a1                ; $f487: 30 18
loc_f489:
	lda $f8                  ; $f489: a5 f8
	sta $2b                  ; $f48b: 85 2b HMCLR
	sta $21                  ; $f48d: 85 21 HMP1
	lda #$0f                 ; $f48f: a9 0f PF2
	sta $f8                  ; $f491: 85 f8
	bne $f466                ; $f493: d0 d1
loc_f495:
	lda #$00                 ; $f495: a9 00 VSYNC
	sta $1b                  ; $f497: 85 1b GRP0
	beq $f475                ; $f499: f0 da
loc_f49b:
	lda #$00                 ; $f49b: a9 00 VSYNC
	sta $1b                  ; $f49d: 85 1b GRP0
	beq $f4b2                ; $f49f: f0 11
loc_f4a1:
	dey                      ; $f4a1: 88
	sty $f6                  ; $f4a2: 84 f6
	cpy #$16                 ; $f4a4: c0 16 AUDC1
	bcs $f49b                ; $f4a6: b0 f3
loc_f4a8:
	lda ($b5),y              ; $f4a8: b1 b5
	sta $1b                  ; $f4aa: 85 1b GRP0
	lda ($b9),y              ; $f4ac: b1 b9
	eor $87                  ; $f4ae: 45 87
	and $88                  ; $f4b0: 25 88
loc_f4b2:
	ldy #$0f                 ; $f4b2: a0 0f PF2
	sta $f7                  ; $f4b4: 85 f7
	lda ($bd),y              ; $f4b6: b1 bd
	ldx #$00                 ; $f4b8: a2 00 VSYNC
	stx $05                  ; $f4ba: 86 05 NUSIZ1
	sta $02                  ; $f4bc: 85 02 WSYNC
	sta $2a                  ; $f4be: 85 2a HMOVE
	sta $1c                  ; $f4c0: 85 1c GRP1
	lda $f7                  ; $f4c2: a5 f7
	sta $06                  ; $f4c4: 85 06 COLUP0
	stx $0d                  ; $f4c6: 86 0d PF0
	lda #$42                 ; $f4c8: a9 42 WSYNC
	and $88                  ; $f4ca: 25 88
	sta $07                  ; $f4cc: 85 07 COLUP1
	stx $0e                  ; $f4ce: 86 0e PF1
	stx $0f                  ; $f4d0: 86 0f PF2
	lda $9d                  ; $f4d2: a5 9d
	sta $1f                  ; $f4d4: 85 1f ENABL
	dey                      ; $f4d6: 88
	sty $f7                  ; $f4d7: 84 f7
	ldx $f6                  ; $f4d9: a6 f6
	dex                      ; $f4db: ca
	txa                      ; $f4dc: 8a
	tay                      ; $f4dd: a8
	cpy #$16                 ; $f4de: c0 16 AUDC1
	bcs $f537                ; $f4e0: b0 55
loc_f4e2:
	lda ($b5),y              ; $f4e2: b1 b5
	sta $1b                  ; $f4e4: 85 1b GRP0
	lda ($b9),y              ; $f4e6: b1 b9
	eor $87                  ; $f4e8: 45 87
	and $88                  ; $f4ea: 25 88
	ldy $f7                  ; $f4ec: a4 f7
	sta $2a                  ; $f4ee: 85 2a HMOVE
	sta $06                  ; $f4f0: 85 06 COLUP0
	lda ($bd),y              ; $f4f2: b1 bd
	sta $1c                  ; $f4f4: 85 1c GRP1
	lda ($bf),y              ; $f4f6: b1 bf
	and $88                  ; $f4f8: 25 88
	sta $07                  ; $f4fa: 85 07 COLUP1
	lda $fc95,y              ; $f4fc: b9 95 fc
	and $9d                  ; $f4ff: 25 9d
	sta $1f                  ; $f501: 85 1f ENABL
	dec $f7                  ; $f503: c6 f7
	bpl $f4db                ; $f505: 10 d4
loc_f507:
	nop                      ; $f507: ea
	dex                      ; $f508: ca
	txa                      ; $f509: 8a
	tay                      ; $f50a: a8
	cpy #$16                 ; $f50b: c0 16 AUDC1
	bcs $f53e                ; $f50d: b0 2f
loc_f50f:
	lda ($b5),y              ; $f50f: b1 b5
	sta $1b                  ; $f511: 85 1b GRP0
	lda ($b9),y              ; $f513: b1 b9
	eor $87                  ; $f515: 45 87
	and $88                  ; $f517: 25 88
	ldy $f8                  ; $f519: a4 f8
	sta $2a                  ; $f51b: 85 2a HMOVE
	sta $06                  ; $f51d: 85 06 COLUP0
	lda ($c1),y              ; $f51f: b1 c1
	sta $1c                  ; $f521: 85 1c GRP1
	lda ($c3),y              ; $f523: b1 c3
	and $88                  ; $f525: 25 88
	sta $07                  ; $f527: 85 07 COLUP1
	lda $fc95,y              ; $f529: b9 95 fc
	and $9d                  ; $f52c: 25 9d
	sta $001f                ; $f52e: 8d 1f 00 ENABL
	dec $f8                  ; $f531: c6 f8
	bpl $f508                ; $f533: 10 d3
loc_f535:
	bmi $f546                ; $f535: 30 0f
loc_f537:
	lda #$00                 ; $f537: a9 00 VSYNC
	sta $1b                  ; $f539: 85 1b GRP0
	nop                      ; $f53b: ea
	beq $f4e8                ; $f53c: f0 aa
loc_f53e:
	lda #$00                 ; $f53e: a9 00 VSYNC
	sta $1b                  ; $f540: 85 1b GRP0
	nop                      ; $f542: ea
	nop                      ; $f543: ea
	beq $f515                ; $f544: f0 cf
loc_f546:
	ldx #$ff                 ; $f546: a2 ff
	sta $02                  ; $f548: 85 02 WSYNC
	sta $2a                  ; $f54a: 85 2a HMOVE
	stx $0d                  ; $f54c: 86 0d PF0
	stx $0e                  ; $f54e: 86 0e PF1
	stx $0f                  ; $f550: 86 0f PF2
	inx                      ; $f552: e8
	stx $1f                  ; $f553: 86 1f ENABL
	stx $1b                  ; $f555: 86 1b GRP0
	stx $1c                  ; $f557: 86 1c GRP1
	stx $1b                  ; $f559: 86 1b GRP0
	stx $f8                  ; $f55b: 86 f8
	ldy #$08                 ; $f55d: a0 08 COLUPF
	lda $9e                  ; $f55f: a5 9e
	ldx $e0                  ; $f561: a6 e0
	beq $f567                ; $f563: f0 02
loc_f565:
	lda #$00                 ; $f565: a9 00 VSYNC
loc_f567:
	lsr                      ; $f567: 4a
	lsr                      ; $f568: 4a
	lsr                      ; $f569: 4a
	cmp #$14                 ; $f56a: c9 14 RESBL
	bcs $f577                ; $f56c: b0 09
loc_f56e:
	ldy #$00                 ; $f56e: a0 00 VSYNC
	cmp #$0c                 ; $f570: c9 0c REFP1
	bcc $f577                ; $f572: 90 03
loc_f574:
	sbc #$0c                 ; $f574: e9 0c REFP1
	tay                      ; $f576: a8
loc_f577:
	tya                      ; $f577: 98
	clc                      ; $f578: 18
	adc #$a8                 ; $f579: 69 a8
	ldx #$0a                 ; $f57b: a2 0a CTRLPF
	sta $02                  ; $f57d: 85 02 WSYNC
	sta $2a                  ; $f57f: 85 2a HMOVE
	sta $c5,x                ; $f581: 95 c5
	sec                      ; $f583: 38
	sbc #$10                 ; $f584: e9 10 RESP0
	dex                      ; $f586: ca
	dex                      ; $f587: ca
	bpl $f57d                ; $f588: 10 f3
loc_f58a:
	lda $8d                  ; $f58a: a5 8d
	sta $08                  ; $f58c: 85 08 COLUPF
	jsr $f30a                ; $f58e: 20 0a f3
loc_f591:
	lda $9e                  ; $f591: a5 9e
	beq $f59b                ; $f593: f0 06
loc_f595:
	dec $9e                  ; $f595: c6 9e
	bne $f59b                ; $f597: d0 02
loc_f599:
	dec $9e                  ; $f599: c6 9e
loc_f59b:
	lda #$20                 ; $f59b: a9 20 HMP0
	ldx #$82                 ; $f59d: a2 82
	sta $02                  ; $f59f: 85 02 WSYNC
	sta $0296                ; $f5a1: 8d 96 02 TIM64T
	stx $01                  ; $f5a4: 86 01 VBLANK
	lda $e0                  ; $f5a6: a5 e0
	cmp #$52                 ; $f5a8: c9 52 RESM0
	bne $f5d6                ; $f5aa: d0 2a
loc_f5ac:
	lda $80                  ; $f5ac: a5 80
	beq $f5d6                ; $f5ae: f0 26
loc_f5b0:
	asl                      ; $f5b0: 0a
	asl                      ; $f5b1: 0a
	sta $80                  ; $f5b2: 85 80
	lda #$00                 ; $f5b4: a9 00 VSYNC
	sta $e5                  ; $f5b6: 85 e5
	sta $9e                  ; $f5b8: 85 9e
	sta $2c                  ; $f5ba: 85 2c CXCLR
	ldy #$df                 ; $f5bc: a0 df
	sty $e8                  ; $f5be: 84 e8
	lda #$14                 ; $f5c0: a9 14 RESBL
	sta $e1                  ; $f5c2: 85 e1
	ldx #$20                 ; $f5c4: a2 20 HMP0
	lda $e9                  ; $f5c6: a5 e9
	cmp #$47                 ; $f5c8: c9 47 COLUP1
	bcc $f5d2                ; $f5ca: 90 06
loc_f5cc:
	ldy #$40                 ; $f5cc: a0 40 VSYNC
	lda #$4c                 ; $f5ce: a9 4c REFP1
	sta $e3                  ; $f5d0: 85 e3
loc_f5d2:
	stx $e7                  ; $f5d2: 86 e7
	sty $e9                  ; $f5d4: 84 e9
loc_f5d6:
	ldy #$00                 ; $f5d6: a0 00 VSYNC
	ldx $e0                  ; $f5d8: a6 e0
	beq $f5f1                ; $f5da: f0 15
loc_f5dc:
	inc $f3                  ; $f5dc: e6 f3
	lda $f3                  ; $f5de: a5 f3
	and #$03                 ; $f5e0: 29 03 RSYNC
	bne $f5e6                ; $f5e2: d0 02
loc_f5e4:
	inc $e0                  ; $f5e4: e6 e0
loc_f5e6:
	lda $fc35,x              ; $f5e6: bd 35 fc
	bpl $f5ed                ; $f5e9: 10 02
loc_f5eb:
	sty $e0                  ; $f5eb: 84 e0
loc_f5ed:
	sta $17                  ; $f5ed: 85 17 AUDF0
	ldy #$01                 ; $f5ef: a0 01 VBLANK
loc_f5f1:
	sty $15                  ; $f5f1: 84 15 AUDC0
	lda #$04                 ; $f5f3: a9 04 NUSIZ0
	sta $19                  ; $f5f5: 85 19 AUDV0
	lda $ec                  ; $f5f7: a5 ec
	bne $f641                ; $f5f9: d0 46
loc_f5fb:
	lda $e9                  ; $f5fb: a5 e9
	cmp #$20                 ; $f5fd: c9 20 HMP0
	bne $f641                ; $f5ff: d0 40
loc_f601:
	ldx $94                  ; $f601: a6 94
	cpx #$04                 ; $f603: e0 04 NUSIZ0
	bne $f60e                ; $f605: d0 07
loc_f607:
	bit $d3                  ; $f607: 24 d3
	bpl $f614                ; $f609: 10 09
loc_f60b:
	dex                      ; $f60b: ca
	bne $f614                ; $f60c: d0 06
loc_f60e:
	cpx #$03                 ; $f60e: e0 03 RSYNC
	bcc $f614                ; $f610: 90 02
loc_f612:
	ldx #$02                 ; $f612: a2 02 WSYNC
loc_f614:
	txa                      ; $f614: 8a
	asl                      ; $f615: 0a
	asl                      ; $f616: 0a
	asl                      ; $f617: 0a
	tax                      ; $f618: aa
	ldy #$03                 ; $f619: a0 03 RSYNC
	lda $fcd7,x              ; $f61b: bd d7 fc
	beq $f641                ; $f61e: f0 21
loc_f620:
	clc                      ; $f620: 18
	adc $f4                  ; $f621: 65 f4
	cmp $e1                  ; $f623: c5 e1
	bcs $f631                ; $f625: b0 0a
loc_f627:
	lda $fcd8,x              ; $f627: bd d8 fc
	sec                      ; $f62a: 38
	sbc $f4                  ; $f62b: e5 f4
	cmp $e1                  ; $f62d: c5 e1
	bcs $f638                ; $f62f: b0 07
loc_f631:
	inx                      ; $f631: e8
	inx                      ; $f632: e8
	dey                      ; $f633: 88
	bpl $f61b                ; $f634: 10 e5
loc_f636:
	bmi $f641                ; $f636: 30 09
loc_f638:
	inc $e9                  ; $f638: e6 e9
	ldx #$20                 ; $f63a: a2 20 HMP0
	stx $e7                  ; $f63c: 86 e7
	dex                      ; $f63e: ca
	stx $e8                  ; $f63f: 86 e8
loc_f641:
	lda $f5                  ; $f641: a5 f5
	bne $f658                ; $f643: d0 13
loc_f645:
	bit $85                  ; $f645: 24 85
	bvc $f658                ; $f647: 50 0f
loc_f649:
	lda $e7                  ; $f649: a5 e7
	beq $f658                ; $f64b: f0 0b
loc_f64d:
	ldx $ea                  ; $f64d: a6 ea
	bne $f658                ; $f64f: d0 07
loc_f651:
	stx $e7                  ; $f651: 86 e7
	inx                      ; $f653: e8
	stx $ea                  ; $f654: 86 ea
	stx $e0                  ; $f656: 86 e0
loc_f658:
	lda $0284                ; $f658: ad 84 02 INTIM
	bne $f658                ; $f65b: d0 fb
loc_f65d:
	sta $16                  ; $f65d: 85 16 AUDC1
	ldy #$82                 ; $f65f: a0 82
	sty $02                  ; $f661: 84 02 WSYNC
	sty $00                  ; $f663: 84 00 VSYNC
	sty $02                  ; $f665: 84 02 WSYNC
	sty $02                  ; $f667: 84 02 WSYNC
	sty $02                  ; $f669: 84 02 WSYNC
	sta $00                  ; $f66b: 85 00 VSYNC
	inc $d2                  ; $f66d: e6 d2
	bne $f678                ; $f66f: d0 07
loc_f671:
	inc $d1                  ; $f671: e6 d1
	bne $f678                ; $f673: d0 03
loc_f675:
	sec                      ; $f675: 38
	ror $d1                  ; $f676: 66 d1
loc_f678:
	ldy #$ff                 ; $f678: a0 ff
	lda $0282                ; $f67a: ad 82 02 SWCHB
	and #$08                 ; $f67d: 29 08 COLUPF
	bne $f683                ; $f67f: d0 02
loc_f681:
	ldy #$0f                 ; $f681: a0 0f PF2
loc_f683:
	tya                      ; $f683: 98
	ldy #$00                 ; $f684: a0 00 VSYNC
	bit $d1                  ; $f686: 24 d1
	bpl $f68e                ; $f688: 10 04
loc_f68a:
	and #$f7                 ; $f68a: 29 f7
	ldy $d1                  ; $f68c: a4 d1
loc_f68e:
	sty $87                  ; $f68e: 84 87
	asl $87                  ; $f690: 06 87
	sta $88                  ; $f692: 85 88
	lda #$2f                 ; $f694: a9 2f
	sta $02                  ; $f696: 85 02 WSYNC
	sta $0296                ; $f698: 8d 96 02 TIM64T
	lda $0280                ; $f69b: ad 80 02 SWCHA
	lsr                      ; $f69e: 4a
	lsr                      ; $f69f: 4a
	lsr                      ; $f6a0: 4a
	lsr                      ; $f6a1: 4a
	sta $83                  ; $f6a2: 85 83
	cmp #$0f                 ; $f6a4: c9 0f PF2
	beq $f6b4                ; $f6a6: f0 0c
loc_f6a8:
	ldx #$00                 ; $f6a8: a2 00 VSYNC
	stx $d1                  ; $f6aa: 86 d1
	lda $d8                  ; $f6ac: a5 d8
	cmp #$20                 ; $f6ae: c9 20 HMP0
	bne $f6b4                ; $f6b0: d0 02
loc_f6b2:
	stx $9e                  ; $f6b2: 86 9e
loc_f6b4:
	lda $0282                ; $f6b4: ad 82 02 SWCHB
	lsr                      ; $f6b7: 4a
	bcs $f6bf                ; $f6b8: b0 05
loc_f6ba:
	ldx #$d1                 ; $f6ba: a2 d1
	jmp $f004                ; $f6bc: 4c 04 f0

; === Code Block $f6bf-$f6c3 ===
.org $f6bf

loc_f6bf:
	lda $9e                  ; $f6bf: a5 9e
	beq $f6c6                ; $f6c1: f0 03
loc_f6c3:
	jmp $f9dc                ; $f6c3: 4c dc f9

; === Code Block $f6c6-$f70e ===
.org $f6c6

loc_f6c6:
	inc $d3                  ; $f6c6: e6 d3
	lda $82                  ; $f6c8: a5 82
	asl                      ; $f6ca: 0a
	eor $82                  ; $f6cb: 45 82
	asl                      ; $f6cd: 0a
	rol $82                  ; $f6ce: 26 82
	lda $ec                  ; $f6d0: a5 ec
	bne $f717                ; $f6d2: d0 43
loc_f6d4:
	ldx $e7                  ; $f6d4: a6 e7
	beq $f717                ; $f6d6: f0 3f
loc_f6d8:
	lda $e9                  ; $f6d8: a5 e9
	sec                      ; $f6da: 38
	sbc $feca,x              ; $f6db: fd ca fe
	sta $e9                  ; $f6de: 85 e9
	inc $e7                  ; $f6e0: e6 e7
	lda $e7                  ; $f6e2: a5 e7
	cmp #$21                 ; $f6e4: c9 21 HMP1
	bcc $f6ec                ; $f6e6: 90 04
loc_f6e8:
	lda #$20                 ; $f6e8: a9 20 HMP0
	sta $e7                  ; $f6ea: 85 e7
loc_f6ec:
	ldx $e9                  ; $f6ec: a6 e9
	cpx #$20                 ; $f6ee: e0 20 HMP0
	beq $f711                ; $f6f0: f0 1f
loc_f6f2:
	ldy $9d                  ; $f6f2: a4 9d
	beq $f703                ; $f6f4: f0 0d
loc_f6f6:
	cpx #$22                 ; $f6f6: e0 22 HMM0
	bne $f703                ; $f6f8: d0 09
loc_f6fa:
	lda #$53                 ; $f6fa: a9 53 RESM1
	sta $e0                  ; $f6fc: 85 e0
	lda #$00                 ; $f6fe: a9 00 VSYNC
	jsr $f3d3                ; $f700: 20 d3 f3
loc_f703:
	cpx #$56                 ; $f703: e0 56 AUDC1
	beq $f711                ; $f705: f0 0a
loc_f707:
	cpx #$36                 ; $f707: e0 36
	bne $f717                ; $f709: d0 0c
loc_f70b:
	tya                      ; $f70b: 98
	bne $f717                ; $f70c: d0 09
loc_f70e:
	jmp $fccc                ; $f70e: 4c cc fc

; === Code Block $f711-$f717 ===
.org $f711

loc_f711:
	lda #$00                 ; $f711: a9 00 VSYNC
	sta $e7                  ; $f713: 85 e7
	sta $f5                  ; $f715: 85 f5

; === Code Block $f717-$f787 ===
.org $f717

loc_f717:
	dec $da                  ; $f717: c6 da
	bpl $f73a                ; $f719: 10 1f
loc_f71b:
	lda #$3b                 ; $f71b: a9 3b
	sta $da                  ; $f71d: 85 da
	sed                      ; $f71f: f8
	lda $d9                  ; $f720: a5 d9
	sec                      ; $f722: 38
	sbc #$01                 ; $f723: e9 01 VBLANK
	bcs $f729                ; $f725: b0 02
loc_f727:
	lda #$59                 ; $f727: a9 59 AUDV0
loc_f729:
	sta $d9                  ; $f729: 85 d9
	lda $d8                  ; $f72b: a5 d8
	sbc #$00                 ; $f72d: e9 00 VSYNC
	sta $d8                  ; $f72f: 85 d8
	cld                      ; $f731: d8
	lda $d8                  ; $f732: a5 d8
	ora $d9                  ; $f734: 05 d9
	bne $f73a                ; $f736: d0 02
loc_f738:
	dec $9e                  ; $f738: c6 9e
loc_f73a:
	lda $07                  ; $f73a: a5 07 COLUP1
	bmi $f744                ; $f73c: 30 06
loc_f73e:
	lda #$00                 ; $f73e: a9 00 VSYNC
	sta $f2                  ; $f740: 85 f2
	beq $f7a9                ; $f742: f0 65
loc_f744:
	lda $e9                  ; $f744: a5 e9
	cmp #$40                 ; $f746: c9 40 VSYNC
	bcs $f7ac                ; $f748: b0 62
loc_f74a:
	lda $ea                  ; $f74a: a5 ea
	bne $f7a9                ; $f74c: d0 5b
loc_f74e:
	lda $94                  ; $f74e: a5 94
	cmp #$04                 ; $f750: c9 04 NUSIZ0
	beq $f7a9                ; $f752: f0 55
loc_f754:
	cmp #$05                 ; $f754: c9 05 NUSIZ1
	bne $f781                ; $f756: d0 29
loc_f758:
	jsr $fca9                ; $f758: 20 a9 fc
loc_f75b:
	bne $f7a9                ; $f75b: d0 4c
loc_f75d:
	sta $ed,x                ; $f75d: 95 ed
	dec $f1                  ; $f75f: c6 f1
	bpl $f765                ; $f761: 10 02
loc_f763:
	dec $9e                  ; $f763: c6 9e
loc_f765:
	lda $93                  ; $f765: a5 93
	and #$03                 ; $f767: 29 03 RSYNC
	asl                      ; $f769: 0a
	asl                      ; $f76a: 0a
	asl                      ; $f76b: 0a
	asl                      ; $f76c: 0a
	adc #$20                 ; $f76d: 69 20 HMP0
	sed                      ; $f76f: f8
	adc $d6                  ; $f770: 65 d6
	sta $d6                  ; $f772: 85 d6
	lda #$00                 ; $f774: a9 00 VSYNC
	adc $d5                  ; $f776: 65 d5
	sta $d5                  ; $f778: 85 d5
	cld                      ; $f77a: d8
	lda #$25                 ; $f77b: a9 25 VDELP0
	sta $e0                  ; $f77d: 85 e0
	bne $f7a9                ; $f77f: d0 28
loc_f781:
	lda $93                  ; $f781: a5 93
	cmp #$06                 ; $f783: c9 06 COLUP0
	bcc $f78a                ; $f785: 90 03
loc_f787:
	jmp $fccc                ; $f787: 4c cc fc

; === Code Block $f78a-$f7a9 ===
.org $f78a

loc_f78a:
	lda $ec                  ; $f78a: a5 ec
	beq $f792                ; $f78c: f0 04
loc_f78e:
	inc $ec                  ; $f78e: e6 ec
	bne $f7a6                ; $f790: d0 14
loc_f792:
	lda $e9                  ; $f792: a5 e9
	cmp #$21                 ; $f794: c9 21 HMP1
	bcs $f7a9                ; $f796: b0 11
loc_f798:
	lda #$05                 ; $f798: a9 05 NUSIZ1
	sta $f2                  ; $f79a: 85 f2
	lda $93                  ; $f79c: a5 93
	and #$04                 ; $f79e: 29 04 NUSIZ0
	bne $f7a6                ; $f7a0: d0 04
loc_f7a2:
	lda #$0f                 ; $f7a2: a9 0f PF2
	sta $83                  ; $f7a4: 85 83
loc_f7a6:
	jsr $f3cd                ; $f7a6: 20 cd f3

; === Code Block $f7a9-$f7a9 ===
.org $f7a9

loc_f7a9:
	jmp $f7d0                ; $f7a9: 4c d0 f7

; === Code Block $f7ac-$f90b ===
.org $f7ac

loc_f7ac:
	lda $bd                  ; $f7ac: a5 bd
	cmp #$b0                 ; $f7ae: c9 b0
	bne $f787                ; $f7b0: d0 d5
loc_f7b2:
	lda #$01                 ; $f7b2: a9 01 VBLANK
	sta $16                  ; $f7b4: 85 16 AUDC1
	lda $e1                  ; $f7b6: a5 e1
	cmp #$8c                 ; $f7b8: c9 8c
	bcs $f7c4                ; $f7ba: b0 08
loc_f7bc:
	cmp #$0d                 ; $f7bc: c9 0d PF0
	bcc $f7ca                ; $f7be: 90 0a
loc_f7c0:
	cmp #$50                 ; $f7c0: c9 50 RESP0
	bcs $f7ca                ; $f7c2: b0 06
loc_f7c4:
	inc $e1                  ; $f7c4: e6 e1
	ldx #$07                 ; $f7c6: a2 07 COLUP1
	bne $f7ce                ; $f7c8: d0 04
loc_f7ca:
	dec $e1                  ; $f7ca: c6 e1
	ldx #$0b                 ; $f7cc: a2 0b REFP0
loc_f7ce:
	stx $e8                  ; $f7ce: 86 e8
loc_f7d0:
	lda $df                  ; $f7d0: a5 df
	asl                      ; $f7d2: 0a
	lda $de                  ; $f7d3: a5 de
	rol                      ; $f7d5: 2a
	bpl $f7da                ; $f7d6: 10 02
loc_f7d8:
	eor #$ff                 ; $f7d8: 49 ff
loc_f7da:
	sta $dc                  ; $f7da: 85 dc
	ldy #$f0                 ; $f7dc: a0 f0
	lda $de                  ; $f7de: a5 de
	bmi $f7e4                ; $f7e0: 30 02
loc_f7e2:
	ldy #$10                 ; $f7e2: a0 10 RESP0
loc_f7e4:
	sty $dd                  ; $f7e4: 84 dd
	sec                      ; $f7e6: 38
	lda #$8f                 ; $f7e7: a9 8f
	sbc $dc                  ; $f7e9: e5 dc
	clc                      ; $f7eb: 18
	adc $df                  ; $f7ec: 65 df
	sta $df                  ; $f7ee: 85 df
	bcc $f7f8                ; $f7f0: 90 06
loc_f7f2:
	lda $de                  ; $f7f2: a5 de
	adc #$03                 ; $f7f4: 69 03 RSYNC
	sta $de                  ; $f7f6: 85 de
loc_f7f8:
	lda $dc                  ; $f7f8: a5 dc
	lsr                      ; $f7fa: 4a
	lsr                      ; $f7fb: 4a
	lsr                      ; $f7fc: 4a
	cmp #$05                 ; $f7fd: c9 05 NUSIZ1
	bcs $f803                ; $f7ff: b0 02
loc_f801:
	lda #$06                 ; $f801: a9 06 COLUP0
loc_f803:
	adc #$04                 ; $f803: 69 04 NUSIZ0
	sta $92                  ; $f805: 85 92
	lda $e7                  ; $f807: a5 e7
	beq $f80f                ; $f809: f0 04
loc_f80b:
	cmp #$03                 ; $f80b: c9 03 RSYNC
	bcc $f830                ; $f80d: 90 21
loc_f80f:
	ora $ec                  ; $f80f: 05 ec
	ora $f2                  ; $f811: 05 f2
	ora $ea                  ; $f813: 05 ea
	bne $f834                ; $f815: d0 1d
loc_f817:
	lda $0c                  ; $f817: a5 0c REFP1
	and #$80                 ; $f819: 29 80
	cmp $84                  ; $f81b: c5 84
	sta $84                  ; $f81d: 85 84
	beq $f834                ; $f81f: f0 13
loc_f821:
	tax                      ; $f821: aa
	bmi $f834                ; $f822: 30 10
loc_f824:
	lda #$01                 ; $f824: a9 01 VBLANK
	sta $e7                  ; $f826: 85 e7
	sta $d1                  ; $f828: 85 d1
	lda #$20                 ; $f82a: a9 20 HMP0
	sta $e0                  ; $f82c: 85 e0
	dec $e9                  ; $f82e: c6 e9
loc_f830:
	lda $83                  ; $f830: a5 83
	sta $e8                  ; $f832: 85 e8
loc_f834:
	lda $ea                  ; $f834: a5 ea
	beq $f850                ; $f836: f0 18
loc_f838:
	lda $83                  ; $f838: a5 83
	and #$02                 ; $f83a: 29 02 WSYNC
	bne $f850                ; $f83c: d0 12
loc_f83e:
	sta $ea                  ; $f83e: 85 ea
	lda #$10                 ; $f840: a9 10 RESP0
	sta $e7                  ; $f842: 85 e7
	sta $f5                  ; $f844: 85 f5
	ldy #$07                 ; $f846: a0 07 COLUP1
	lda $dd                  ; $f848: a5 dd
	bmi $f84e                ; $f84a: 30 02
loc_f84c:
	ldy #$0b                 ; $f84c: a0 0b REFP0
loc_f84e:
	sty $e8                  ; $f84e: 84 e8
loc_f850:
	lda $ec                  ; $f850: a5 ec
	bne $f884                ; $f852: d0 30
loc_f854:
	lda $9d                  ; $f854: a5 9d
	beq $f884                ; $f856: f0 2c
loc_f858:
	lda $e1                  ; $f858: a5 e1
	sec                      ; $f85a: 38
	sbc #$44                 ; $f85b: e9 44 NUSIZ0
	cmp #$0f                 ; $f85d: c9 0f PF2
	bcs $f884                ; $f85f: b0 23
loc_f861:
	lda $e9                  ; $f861: a5 e9
	cmp #$54                 ; $f863: c9 54 RESBL
	bcc $f870                ; $f865: 90 09
loc_f867:
	lda $83                  ; $f867: a5 83
	lsr                      ; $f869: 4a
	bcs $f870                ; $f86a: b0 04
loc_f86c:
	lda #$15                 ; $f86c: a9 15 AUDC0
	bne $f87e                ; $f86e: d0 0e
loc_f870:
	lda $e9                  ; $f870: a5 e9
	cmp #$20                 ; $f872: c9 20 HMP0
	bne $f884                ; $f874: d0 0e
loc_f876:
	lda $83                  ; $f876: a5 83
	and #$02                 ; $f878: 29 02 WSYNC
	bne $f884                ; $f87a: d0 08
loc_f87c:
	lda #$0c                 ; $f87c: a9 0c REFP1
loc_f87e:
	sta $ec                  ; $f87e: 85 ec
	lda #$4c                 ; $f880: a9 4c REFP1
	sta $e1                  ; $f882: 85 e1
loc_f884:
	lda $ea                  ; $f884: a5 ea
	beq $f89f                ; $f886: f0 17
loc_f888:
	lda $dc                  ; $f888: a5 dc
	lsr                      ; $f88a: 4a
	lsr                      ; $f88b: 4a
	clc                      ; $f88c: 18
	ldy $dd                  ; $f88d: a4 dd
	bmi $f894                ; $f88f: 30 03
loc_f891:
	eor #$ff                 ; $f891: 49 ff
	sec                      ; $f893: 38
loc_f894:
	adc #$4b                 ; $f894: 69 4b REFP0
	sta $e1                  ; $f896: 85 e1
	lda #$29                 ; $f898: a9 29 RESMP1
	sec                      ; $f89a: 38
	sbc $92                  ; $f89b: e5 92
	sta $e9                  ; $f89d: 85 e9
loc_f89f:
	lda $ec                  ; $f89f: a5 ec
	beq $f8e0                ; $f8a1: f0 3d
loc_f8a3:
	lda #$00                 ; $f8a3: a9 00 VSYNC
	sta $e7                  ; $f8a5: 85 e7
	lda $d3                  ; $f8a7: a5 d3
	and #$07                 ; $f8a9: 29 07 COLUP1
	bne $f8d5                ; $f8ab: d0 28
loc_f8ad:
	lda $83                  ; $f8ad: a5 83
	lsr                      ; $f8af: 4a
	bcs $f8b4                ; $f8b0: b0 02
loc_f8b2:
	dec $ec                  ; $f8b2: c6 ec
loc_f8b4:
	lsr                      ; $f8b4: 4a
	bcs $f8b9                ; $f8b5: b0 02
loc_f8b7:
	inc $ec                  ; $f8b7: e6 ec
loc_f8b9:
	lda $ec                  ; $f8b9: a5 ec
	cmp #$0b                 ; $f8bb: c9 0b REFP0
	bcs $f8c5                ; $f8bd: b0 06
loc_f8bf:
	lda #$0f                 ; $f8bf: a9 0f PF2
	sta $e8                  ; $f8c1: 85 e8
	lda #$0b                 ; $f8c3: a9 0b REFP0
loc_f8c5:
	cmp #$16                 ; $f8c5: c9 16 AUDC1
	bcc $f8d3                ; $f8c7: 90 0a
loc_f8c9:
	lda #$00                 ; $f8c9: a9 00 VSYNC
	ldx #$05                 ; $f8cb: a2 05 NUSIZ1
	stx $e4                  ; $f8cd: 86 e4
	ldx #$56                 ; $f8cf: a2 56 AUDC1
	stx $e9                  ; $f8d1: 86 e9
loc_f8d3:
	sta $ec                  ; $f8d3: 85 ec
loc_f8d5:
	lda $ec                  ; $f8d5: a5 ec
	beq $f8e0                ; $f8d7: f0 07
loc_f8d9:
	asl                      ; $f8d9: 0a
	sec                      ; $f8da: 38
	rol                      ; $f8db: 2a
	adc #$01                 ; $f8dc: 69 01 VBLANK
	sta $e9                  ; $f8de: 85 e9
loc_f8e0:
	lda $ea                  ; $f8e0: a5 ea
	bne $f947                ; $f8e2: d0 63
loc_f8e4:
	lda $ec                  ; $f8e4: a5 ec
	cmp #$0c                 ; $f8e6: c9 0c REFP1
	bcs $f947                ; $f8e8: b0 5d
loc_f8ea:
	lda $d3                  ; $f8ea: a5 d3
	and #$03                 ; $f8ec: 29 03 RSYNC
	tax                      ; $f8ee: aa
	lsr                      ; $f8ef: 4a
	bcs $f947                ; $f8f0: b0 55
loc_f8f2:
	lda $e8                  ; $f8f2: a5 e8
	ldy $e7                  ; $f8f4: a4 e7
	bne $f8fa                ; $f8f6: d0 02
loc_f8f8:
	lda $83                  ; $f8f8: a5 83
loc_f8fa:
	lsr                      ; $f8fa: 4a
	lsr                      ; $f8fb: 4a
	lsr                      ; $f8fc: 4a
	bcs $f90e                ; $f8fd: b0 0f
loc_f8ff:
	dec $e1                  ; $f8ff: c6 e1
	ldy #$08                 ; $f901: a0 08 COLUPF
	sty $e5                  ; $f903: 84 e5
	cpx #$00                 ; $f905: e0 00 VSYNC
	bne $f90b                ; $f907: d0 02
loc_f909:
	dec $e4                  ; $f909: c6 e4
loc_f90b:
	jmp $f91d                ; $f90b: 4c 1d f9

; === Code Block $f90e-$f947 ===
.org $f90e

loc_f90e:
	lsr                      ; $f90e: 4a
	bcs $f91d                ; $f90f: b0 0c
loc_f911:
	inc $e1                  ; $f911: e6 e1
	ldy #$00                 ; $f913: a0 00 VSYNC
	sty $e5                  ; $f915: 84 e5
	cpx #$00                 ; $f917: e0 00 VSYNC
	bne $f91d                ; $f919: d0 02
loc_f91b:
	dec $e4                  ; $f91b: c6 e4
loc_f91d:
	ldx #$00                 ; $f91d: a2 00 VSYNC
	lda $e9                  ; $f91f: a5 e9
	cmp #$40                 ; $f921: c9 40 VSYNC
	bcc $f927                ; $f923: 90 02
loc_f925:
	ldx #$02                 ; $f925: a2 02 WSYNC
loc_f927:
	lda $e1                  ; $f927: a5 e1
	cmp #$08                 ; $f929: c9 08 COLUPF
	bcs $f934                ; $f92b: b0 07
loc_f92d:
	jsr $faab                ; $f92d: 20 ab fa
loc_f930:
	lda #$94                 ; $f930: a9 94
	sta $e1                  ; $f932: 85 e1
loc_f934:
	cmp #$95                 ; $f934: c9 95
	bcc $f93f                ; $f936: 90 07
loc_f938:
	jsr $fead                ; $f938: 20 ad fe
loc_f93b:
	lda #$08                 ; $f93b: a9 08 COLUPF
	sta $e1                  ; $f93d: 85 e1
loc_f93f:
	lda $e4                  ; $f93f: a5 e4
	bpl $f947                ; $f941: 10 04
loc_f943:
	lda #$04                 ; $f943: a9 04 NUSIZ0
	sta $e4                  ; $f945: 85 e4

; === Code Block $f947-$f9dc ===
.org $f947

loc_f947:
	lda $9d                  ; $f947: a5 9d
	bne $f968                ; $f949: d0 1d
loc_f94b:
	ldx #$00                 ; $f94b: a2 00 VSYNC
	lda $e1                  ; $f94d: a5 e1
	sec                      ; $f94f: 38
	sbc $e3                  ; $f950: e5 e3
	beq $f968                ; $f952: f0 14
loc_f954:
	bcs $f958                ; $f954: b0 02
loc_f956:
	ldx #$08                 ; $f956: a2 08 COLUPF
loc_f958:
	lda $d3                  ; $f958: a5 d3
	and #$07                 ; $f95a: 29 07 COLUP1
	bne $f966                ; $f95c: d0 08
loc_f95e:
	inc $e3                  ; $f95e: e6 e3
	bcs $f966                ; $f960: b0 04
loc_f962:
	dec $e3                  ; $f962: c6 e3
	dec $e3                  ; $f964: c6 e3
loc_f966:
	stx $e6                  ; $f966: 86 e6
loc_f968:
	lda $ec                  ; $f968: a5 ec
	cmp #$0b                 ; $f96a: c9 0b REFP0
	bne $f985                ; $f96c: d0 17
loc_f96e:
	lda $83                  ; $f96e: a5 83
	and #$0c                 ; $f970: 29 0c REFP1
	cmp #$0c                 ; $f972: c9 0c REFP1
	beq $f985                ; $f974: f0 0f
loc_f976:
	lda $83                  ; $f976: a5 83
	sta $e8                  ; $f978: 85 e8
	lda #$01                 ; $f97a: a9 01 VBLANK
	sta $e7                  ; $f97c: 85 e7
	lsr                      ; $f97e: 4a
	sta $ec                  ; $f97f: 85 ec
	lda #$1f                 ; $f981: a9 1f ENABL
	sta $e9                  ; $f983: 85 e9
loc_f985:
	ldx $e4                  ; $f985: a6 e4
	lda $83                  ; $f987: a5 83
	and #$0c                 ; $f989: 29 0c REFP1
	cmp #$0c                 ; $f98b: c9 0c REFP1
	bne $f991                ; $f98d: d0 02
loc_f98f:
	ldx #$05                 ; $f98f: a2 05 NUSIZ1
loc_f991:
	lda $e9                  ; $f991: a5 e9
	cmp #$1f                 ; $f993: c9 1f ENABL
	bne $f99b                ; $f995: d0 04
loc_f997:
	ldx #$03                 ; $f997: a2 03 RSYNC
	bne $f9b1                ; $f999: d0 16
loc_f99b:
	cmp #$56                 ; $f99b: c9 56 AUDC1
	beq $f9b1                ; $f99d: f0 12
loc_f99f:
	cmp #$20                 ; $f99f: c9 20 HMP0
	beq $f9b1                ; $f9a1: f0 0e
loc_f9a3:
	ldx #$00                 ; $f9a3: a2 00 VSYNC
	bcc $f9b1                ; $f9a5: 90 0a
loc_f9a7:
	cmp #$3c                 ; $f9a7: c9 3c
	bcs $f9b1                ; $f9a9: b0 06
loc_f9ab:
	lda $ec                  ; $f9ab: a5 ec
	bne $f9b1                ; $f9ad: d0 02
loc_f9af:
	ldx #$05                 ; $f9af: a2 05 NUSIZ1
loc_f9b1:
	lda $ea                  ; $f9b1: a5 ea
	beq $f9b7                ; $f9b3: f0 02
loc_f9b5:
	ldx #$06                 ; $f9b5: a2 06 COLUP0
loc_f9b7:
	lda $ec                  ; $f9b7: a5 ec
	beq $f9c1                ; $f9b9: f0 06
loc_f9bb:
	and #$01                 ; $f9bb: 29 01 VBLANK
	clc                      ; $f9bd: 18
	adc #$07                 ; $f9be: 69 07 COLUP1
	tax                      ; $f9c0: aa
loc_f9c1:
	lda $f2                  ; $f9c1: a5 f2
	beq $f9c7                ; $f9c3: f0 02
loc_f9c5:
	ldx #$00                 ; $f9c5: a2 00 VSYNC
loc_f9c7:
	stx $e4                  ; $f9c7: 86 e4
	lda $fec2,x              ; $f9c9: bd c2 fe
	sta $b5                  ; $f9cc: 85 b5
	lda #$fb                 ; $f9ce: a9 fb
	sta $b6                  ; $f9d0: 85 b6
	lda #$21                 ; $f9d2: a9 21 HMP1
	cpx #$07                 ; $f9d4: e0 07 COLUP1
	bcc $f9da                ; $f9d6: 90 02
loc_f9d8:
	lda #$0b                 ; $f9d8: a9 0b REFP0
loc_f9da:
	sta $b9                  ; $f9da: 85 b9

; === Code Block $f9dc-$fa72 ===
.org $f9dc

loc_f9dc:
	lda $93                  ; $f9dc: a5 93
	tax                      ; $f9de: aa
	ldy $94                  ; $f9df: a4 94
	lda $fca1,y              ; $f9e1: b9 a1 fc
	sta $1f                  ; $f9e4: 85 1f ENABL
	cpy #$05                 ; $f9e6: c0 05 NUSIZ1
	bne $f9fa                ; $f9e8: d0 10
loc_f9ea:
	jsr $fca9                ; $f9ea: 20 a9 fc
loc_f9ed:
	beq $f9f3                ; $f9ed: f0 04
loc_f9ef:
	ldx #$0c                 ; $f9ef: a2 0c REFP1
	bne $f9fa                ; $f9f1: d0 07
loc_f9f3:
	lda $93                  ; $f9f3: a5 93
	and #$03                 ; $f9f5: 29 03 RSYNC
	ora #$08                 ; $f9f7: 09 08 COLUPF
	tax                      ; $f9f9: aa
loc_f9fa:
	lda $82                  ; $f9fa: a5 82
	and $ffd9,x              ; $f9fc: 3d d9 ff
	sta $f6                  ; $f9ff: 85 f6
	lda $feeb,x              ; $fa01: bd eb fe
	clc                      ; $fa04: 18
	adc $f6                  ; $fa05: 65 f6
	sta $b7                  ; $fa07: 85 b7
	lda $ffcd,x              ; $fa09: bd cd ff
	sta $d4                  ; $fa0c: 85 d4
	lda $ffc1,x              ; $fa0e: bd c1 ff
	sta $bb                  ; $fa11: 85 bb
	ldy $94                  ; $fa13: a4 94
	lda $ffee,y              ; $fa15: b9 ee ff
	beq $fa30                ; $fa18: f0 16
loc_fa1a:
	lda #$60                 ; $fa1a: a9 60 HMP0
	bit $d3                  ; $fa1c: 24 d3
	bpl $fa22                ; $fa1e: 10 02
loc_fa20:
	lda #$70                 ; $fa20: a9 70
loc_fa22:
	sta $b7                  ; $fa22: 85 b7
	lda #$30                 ; $fa24: a9 30
	sta $bb                  ; $fa26: 85 bb
	lda $93                  ; $fa28: a5 93
	sta $1f                  ; $fa2a: 85 1f ENABL
	lda #$03                 ; $fa2c: a9 03 RSYNC
	sta $d4                  ; $fa2e: 85 d4
loc_fa30:
	lda $9e                  ; $fa30: a5 9e
	bne $fa60                ; $fa32: d0 2c
loc_fa34:
	lda $ffee,y              ; $fa34: b9 ee ff
	bne $fa60                ; $fa37: d0 27
loc_fa39:
	cpy #$05                 ; $fa39: c0 05 NUSIZ1
	beq $fa60                ; $fa3b: f0 23
loc_fa3d:
	lda $e2                  ; $fa3d: a5 e2
	asl                      ; $fa3f: 0a
	asl                      ; $fa40: 0a
	asl                      ; $fa41: 0a
	and #$30                 ; $fa42: 29 30
	cmp #$30                 ; $fa44: c9 30
	and #$10                 ; $fa46: 29 10 RESP0
	adc $b7                  ; $fa48: 65 b7
	sta $b7                  ; $fa4a: 85 b7
	lda $d3                  ; $fa4c: a5 d3
	lsr                      ; $fa4e: 4a
	bcs $fa60                ; $fa4f: b0 0f
loc_fa51:
	lda $93                  ; $fa51: a5 93
	cmp #$04                 ; $fa53: c9 04 NUSIZ0
	bcs $fa60                ; $fa55: b0 09
loc_fa57:
	ldx $e2                  ; $fa57: a6 e2
	bne $fa5d                ; $fa59: d0 02
loc_fa5b:
	ldx #$a0                 ; $fa5b: a2 a0
loc_fa5d:
	dex                      ; $fa5d: ca
	stx $e2                  ; $fa5e: 86 e2
loc_fa60:
	jsr $fcbf                ; $fa60: 20 bf fc
loc_fa63:
	inx                      ; $fa63: e8
	lda $c5,x                ; $fa64: b5 c5
	bne $fa72                ; $fa66: d0 0a
loc_fa68:
	lda #$58                 ; $fa68: a9 58 AUDF1
	sta $c5,x                ; $fa6a: 95 c5
	inx                      ; $fa6c: e8
	inx                      ; $fa6d: e8
	cpx #$09                 ; $fa6e: e0 09 COLUBK
	bcc $fa64                ; $fa70: 90 f2
loc_fa72:
	jmp $f00f                ; $fa72: 4c 0f f0

; === Code Block $fa75-$fafc ===
.org $fa75

loc_fa75:
	ldx #$01                 ; $fa75: a2 01 VBLANK
	stx $82                  ; $fa77: 86 82
	lda #$04                 ; $fa79: a9 04 NUSIZ0
	sta $19,x                ; $fa7b: 95 19 AUDV0
	lda #$10                 ; $fa7d: a9 10 RESP0
	sta $17,x                ; $fa7f: 95 17 AUDF0
	dex                      ; $fa81: ca
	bpl $fa79                ; $fa82: 10 f5
loc_fa84:
	stx $9e                  ; $fa84: 86 9e
	sta $e1                  ; $fa86: 85 e1
	asl                      ; $fa88: 0a
	sta $e9                  ; $fa89: 85 e9
	sta $d6                  ; $fa8b: 85 d6
	sta $d8                  ; $fa8d: 85 d8
	ldx #$1b                 ; $fa8f: a2 1b GRP0
	lda $fe91,x              ; $fa91: bd 91 fe
	sta $b5,x                ; $fa94: 95 b5
	dex                      ; $fa96: ca
	bpl $fa91                ; $fa97: 10 f8
loc_fa99:
	lda #$3b                 ; $fa99: a9 3b
	sta $da                  ; $fa9b: 85 da
	lda #$1f                 ; $fa9d: a9 1f ENABL
	sta $f1                  ; $fa9f: 85 f1
	lda #$a0                 ; $faa1: a9 a0
	sta $80                  ; $faa3: 85 80
	lda #$c4                 ; $faa5: a9 c4
	sta $81                  ; $faa7: 85 81
	bne $fabe                ; $faa9: d0 13
loc_faab:
	lda $81                  ; $faab: a5 81
	asl                      ; $faad: 0a
	eor $81                  ; $faae: 45 81
	asl                      ; $fab0: 0a
	eor $81                  ; $fab1: 45 81
	asl                      ; $fab3: 0a
	asl                      ; $fab4: 0a
	rol                      ; $fab5: 2a
	eor $81                  ; $fab6: 45 81
	lsr                      ; $fab8: 4a
	ror $81                  ; $fab9: 66 81
	dex                      ; $fabb: ca
	bpl $faab                ; $fabc: 10 ed
loc_fabe:
	lda #$7c                 ; $fabe: a9 7c
	sta $e2                  ; $fac0: 85 e2
	lda $81                  ; $fac2: a5 81
	lsr                      ; $fac4: 4a
	lsr                      ; $fac5: 4a
	lsr                      ; $fac6: 4a
	pha                      ; $fac7: 48
	and #$07                 ; $fac8: 29 07 COLUP1
	sta $94                  ; $faca: 85 94
	pla                      ; $facc: 68
	lsr                      ; $facd: 4a
	lsr                      ; $face: 4a
	lsr                      ; $facf: 4a
	sta $eb                  ; $fad0: 85 eb
	lda $81                  ; $fad2: a5 81
	and #$07                 ; $fad4: 29 07 COLUP1
	sta $93                  ; $fad6: 85 93
	ldx #$4c                 ; $fad8: a2 4c REFP1
	ldy #$00                 ; $fada: a0 00 VSYNC
	lda $94                  ; $fadc: a5 94
	cmp #$02                 ; $fade: c9 02 WSYNC
	bcs $faed                ; $fae0: b0 0b
loc_fae2:
	ldy #$ff                 ; $fae2: a0 ff
	ldx #$11                 ; $fae4: a2 11 RESP1
	lda $81                  ; $fae6: a5 81
	asl                      ; $fae8: 0a
	bcc $faed                ; $fae9: 90 02
loc_faeb:
	ldx #$88                 ; $faeb: a2 88
loc_faed:
	sty $9d                  ; $faed: 84 9d
	stx $e3                  ; $faef: 86 e3
	ldx $94                  ; $faf1: a6 94
	lda $ffee,x              ; $faf3: bd ee ff
	beq $fafc                ; $faf6: f0 04
loc_faf8:
	lda #$3c                 ; $faf8: a9 3c
	sta $e2                  ; $fafa: 85 e2
loc_fafc:
	rts                      ; $fafc: 60

; === Code Block $fca9-$fcbe ===
.org $fca9

loc_fca9:
	lda $81                  ; $fca9: a5 81
	rol                      ; $fcab: 2a
	rol                      ; $fcac: 2a
	rol                      ; $fcad: 2a
	and #$03                 ; $fcae: 29 03 RSYNC
	tax                      ; $fcb0: aa
	ldy $93                  ; $fcb1: a4 93
	lda $fef8,y              ; $fcb3: b9 f8 fe
	tay                      ; $fcb6: a8
	and $ed,x                ; $fcb7: 35 ed
	php                      ; $fcb9: 08
	tya                      ; $fcba: 98
	ora $ed,x                ; $fcbb: 15 ed
	plp                      ; $fcbd: 28
	rts                      ; $fcbe: 60

; === Code Block $fcbf-$fccb ===
.org $fcbf

loc_fcbf:
	ldx #$02                 ; $fcbf: a2 02 WSYNC
	txa                      ; $fcc1: 8a
	asl                      ; $fcc2: 0a
	asl                      ; $fcc3: 0a
	tay                      ; $fcc4: a8
	jsr $f3b6                ; $fcc5: 20 b6 f3
loc_fcc8:
	dex                      ; $fcc8: ca
	bpl $fcc1                ; $fcc9: 10 f6
loc_fccb:
	rts                      ; $fccb: 60

; === Code Block $fccc-$fcd4 ===
.org $fccc

loc_fccc:
	lda #$31                 ; $fccc: a9 31
	sta $e0                  ; $fcce: 85 e0
	lda #$84                 ; $fcd0: a9 84
	sta $9e                  ; $fcd2: 85 9e
	jmp $f9dc                ; $fcd4: 4c dc f9

; === Code Block $fead-$febf ===
.org $fead

loc_fead:
	lda $81                  ; $fead: a5 81
	asl                      ; $feaf: 0a
	eor $81                  ; $feb0: 45 81
	asl                      ; $feb2: 0a
	eor $81                  ; $feb3: 45 81
	asl                      ; $feb5: 0a
	asl                      ; $feb6: 0a
	eor $81                  ; $feb7: 45 81
	asl                      ; $feb9: 0a
	rol $81                  ; $feba: 26 81
	dex                      ; $febc: ca
	bpl $fead                ; $febd: 10 ee
loc_febf:
	jmp $fabe                ; $febf: 4c be fa

; === Vectors ===
.org $fffc
	.word reset
	.word reset
