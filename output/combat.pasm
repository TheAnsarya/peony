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
CTRLPF           = $000a
REFP0            = $000b
REFP1            = $000c
PF0              = $000d
PF1              = $000e
PF2              = $000f
RESP0            = $0010
RESP1            = $0011
AUDC0            = $0015
AUDF0            = $0017
AUDV0            = $0019
GRP0             = $001b
GRP1             = $001c
ENAM0            = $001d
ENAM1            = $001e
ENABL            = $001f
HMP0             = $0020
HMP1             = $0021
VDELP0           = $0025
RESMP0           = $0028
RESMP1           = $0029
HMOVE            = $002a
HMCLR            = $002b
CXCLR            = $002c
COLUP1           = $0047
ENAM0            = $005d
SWCHA            = $0280
SWCHB            = $0282
SWBCNT           = $0283
INTIM            = $0284
TIM64T           = $0296

; === Code Block $f000-$f02f ===
.org $f000

reset:
	sei                      ; $f000: 78
	cld                      ; $f001: d8
	ldx #$ff                 ; $f002: a2 ff
	txs                      ; $f004: 9a
	ldx #$5d                 ; $f005: a2 5d ENAM0
	jsr $f5bd                ; $f007: 20 bd f5
loc_f00a:
	lda #$10                 ; $f00a: a9 10 RESP0
	sta $0283                ; $f00c: 8d 83 02 SWBCNT
	sta $88                  ; $f00f: 85 88
	jsr $f1a3                ; $f011: 20 a3 f1
loc_f014:
	jsr $f032                ; $f014: 20 32 f0
loc_f017:
	jsr $f157                ; $f017: 20 57 f1
loc_f01a:
	jsr $f572                ; $f01a: 20 72 f5
loc_f01d:
	jsr $f2da                ; $f01d: 20 da f2
loc_f020:
	jsr $f444                ; $f020: 20 44 f4
loc_f023:
	jsr $f214                ; $f023: 20 14 f2
loc_f026:
	jsr $f2a9                ; $f026: 20 a9 f2
loc_f029:
	jsr $f1f2                ; $f029: 20 f2 f1
loc_f02c:
	jsr $f054                ; $f02c: 20 54 f0
loc_f02f:
	jmp $f014                ; $f02f: 4c 14 f0

; === Code Block $f032-$f053 ===
.org $f032

loc_f032:
	inc $86                  ; $f032: e6 86
	sta $2b                  ; $f034: 85 2b HMCLR
	lda #$02                 ; $f036: a9 02 WSYNC
	sta $02                  ; $f038: 85 02 WSYNC
	sta $01                  ; $f03a: 85 01 VBLANK
	sta $02                  ; $f03c: 85 02 WSYNC
	sta $02                  ; $f03e: 85 02 WSYNC
	sta $02                  ; $f040: 85 02 WSYNC
	sta $00                  ; $f042: 85 00 VSYNC
	sta $02                  ; $f044: 85 02 WSYNC
	sta $02                  ; $f046: 85 02 WSYNC
	lda #$00                 ; $f048: a9 00 VSYNC
	sta $02                  ; $f04a: 85 02 WSYNC
	sta $00                  ; $f04c: 85 00 VSYNC
	lda #$2b                 ; $f04e: a9 2b HMCLR
	sta $0296                ; $f050: 8d 96 02 TIM64T
	rts                      ; $f053: 60

; === Code Block $f054-$f0ca ===
.org $f054

loc_f054:
	lda #$20                 ; $f054: a9 20 HMP0
	sta $b4                  ; $f056: 85 b4
	sta $02                  ; $f058: 85 02 WSYNC
	sta $2a                  ; $f05a: 85 2a HMOVE
	lda $0284                ; $f05c: ad 84 02 INTIM
	bne $f05c                ; $f05f: d0 fb
loc_f061:
	sta $02                  ; $f061: 85 02 WSYNC
	sta $2c                  ; $f063: 85 2c CXCLR
	sta $01                  ; $f065: 85 01 VBLANK
	tsx                      ; $f067: ba
	stx $d3                  ; $f068: 86 d3
	lda #$02                 ; $f06a: a9 02 WSYNC
	sta $0a                  ; $f06c: 85 0a CTRLPF
	ldx $dc                  ; $f06e: a6 dc
	sta $02                  ; $f070: 85 02 WSYNC
	dex                      ; $f072: ca
	bne $f070                ; $f073: d0 fb
loc_f075:
	lda $dc                  ; $f075: a5 dc
	cmp #$0e                 ; $f077: c9 0e PF1
	beq $f0cd                ; $f079: f0 52
loc_f07b:
	ldx #$05                 ; $f07b: a2 05 NUSIZ1
	lda #$00                 ; $f07d: a9 00 VSYNC
	sta $de                  ; $f07f: 85 de
	sta $df                  ; $f081: 85 df
	sta $02                  ; $f083: 85 02 WSYNC
	lda $de                  ; $f085: a5 de
	sta $0e                  ; $f087: 85 0e PF1
	ldy $e2                  ; $f089: a4 e2
	lda $f5c5,y              ; $f08b: b9 c5 f5
	and #$f0                 ; $f08e: 29 f0
	sta $de                  ; $f090: 85 de
	ldy $e0                  ; $f092: a4 e0
	lda $f5c5,y              ; $f094: b9 c5 f5
	and #$0f                 ; $f097: 29 0f PF2
	ora $de                  ; $f099: 05 de
	sta $de                  ; $f09b: 85 de
	lda $df                  ; $f09d: a5 df
	sta $0e                  ; $f09f: 85 0e PF1
	ldy $e3                  ; $f0a1: a4 e3
	lda $f5c5,y              ; $f0a3: b9 c5 f5
	and #$f0                 ; $f0a6: 29 f0
	sta $df                  ; $f0a8: 85 df
	ldy $e1                  ; $f0aa: a4 e1
	lda $f5c5,y              ; $f0ac: b9 c5 f5
	and $87                  ; $f0af: 25 87
	sta $02                  ; $f0b1: 85 02 WSYNC
	ora $df                  ; $f0b3: 05 df
	sta $df                  ; $f0b5: 85 df
	lda $de                  ; $f0b7: a5 de
	sta $0e                  ; $f0b9: 85 0e PF1
	dex                      ; $f0bb: ca
	bmi $f0cd                ; $f0bc: 30 0f
loc_f0be:
	inc $e0                  ; $f0be: e6 e0
	inc $e2                  ; $f0c0: e6 e2
	inc $e1                  ; $f0c2: e6 e1
	inc $e3                  ; $f0c4: e6 e3
	lda $df                  ; $f0c6: a5 df
	sta $0e                  ; $f0c8: 85 0e PF1
	jmp $f083                ; $f0ca: 4c 83 f0

; === Code Block $f0cd-$f156 ===
.org $f0cd

loc_f0cd:
	lda #$00                 ; $f0cd: a9 00 VSYNC
	sta $0e                  ; $f0cf: 85 0e PF1
	sta $02                  ; $f0d1: 85 02 WSYNC
	lda #$05                 ; $f0d3: a9 05 NUSIZ1
	sta $0a                  ; $f0d5: 85 0a CTRLPF
	lda $d6                  ; $f0d7: a5 d6
	sta $06                  ; $f0d9: 85 06 COLUP0
	lda $d7                  ; $f0db: a5 d7
	sta $07                  ; $f0dd: 85 07 COLUP1
	ldx #$1e                 ; $f0df: a2 1e ENAM1
	txs                      ; $f0e1: 9a
	sec                      ; $f0e2: 38
	lda $a4                  ; $f0e3: a5 a4
	sbc $b4                  ; $f0e5: e5 b4
	and #$fe                 ; $f0e7: 29 fe
	tax                      ; $f0e9: aa
	and #$f0                 ; $f0ea: 29 f0
	beq $f0f2                ; $f0ec: f0 04
loc_f0ee:
	lda #$00                 ; $f0ee: a9 00 VSYNC
	beq $f0f4                ; $f0f0: f0 02
loc_f0f2:
	lda $bd,x                ; $f0f2: b5 bd
loc_f0f4:
	sta $02                  ; $f0f4: 85 02 WSYNC
	sta $1b                  ; $f0f6: 85 1b GRP0
	lda $a7                  ; $f0f8: a5 a7
	eor $b4                  ; $f0fa: 45 b4
	and #$fe                 ; $f0fc: 29 fe
	php                      ; $f0fe: 08
	lda $a6                  ; $f0ff: a5 a6
	eor $b4                  ; $f101: 45 b4
	and #$fe                 ; $f103: 29 fe
	php                      ; $f105: 08
	lda $b4                  ; $f106: a5 b4
	bpl $f10c                ; $f108: 10 02
loc_f10a:
	eor #$f8                 ; $f10a: 49 f8
loc_f10c:
	cmp #$20                 ; $f10c: c9 20 HMP0
	bcc $f114                ; $f10e: 90 04
loc_f110:
	lsr                      ; $f110: 4a
	lsr                      ; $f111: 4a
	lsr                      ; $f112: 4a
	tay                      ; $f113: a8
loc_f114:
	lda $a5                  ; $f114: a5 a5
	sec                      ; $f116: 38
	sbc $b4                  ; $f117: e5 b4
	inc $b4                  ; $f119: e6 b4
	nop                      ; $f11b: ea
	ora #$01                 ; $f11c: 09 01 VBLANK
	tax                      ; $f11e: aa
	and #$f0                 ; $f11f: 29 f0
	beq $f127                ; $f121: f0 04
loc_f123:
	lda #$00                 ; $f123: a9 00 VSYNC
	beq $f129                ; $f125: f0 02
loc_f127:
	lda $bd,x                ; $f127: b5 bd
loc_f129:
	bit $82                  ; $f129: 24 82
	sta $1c                  ; $f12b: 85 1c GRP1
	bmi $f13b                ; $f12d: 30 0c
loc_f12f:
	lda ($b5),y              ; $f12f: b1 b5
	sta $0d                  ; $f131: 85 0d PF0
	lda ($b7),y              ; $f133: b1 b7
	sta $0e                  ; $f135: 85 0e PF1
	lda ($b9),y              ; $f137: b1 b9
	sta $0f                  ; $f139: 85 0f PF2
loc_f13b:
	inc $b4                  ; $f13b: e6 b4
	lda $b4                  ; $f13d: a5 b4
	eor #$ec                 ; $f13f: 49 ec
	bne $f0df                ; $f141: d0 9c
loc_f143:
	ldx $d3                  ; $f143: a6 d3
	txs                      ; $f145: 9a
	sta $1d                  ; $f146: 85 1d ENAM0
	sta $1e                  ; $f148: 85 1e ENAM1
	sta $1b                  ; $f14a: 85 1b GRP0
	sta $1c                  ; $f14c: 85 1c GRP1
	sta $1b                  ; $f14e: 85 1b GRP0
	sta $0d                  ; $f150: 85 0d PF0
	sta $0e                  ; $f152: 85 0e PF1
	sta $0f                  ; $f154: 85 0f PF2
	rts                      ; $f156: 60

; === Code Block $f157-$f1a3 ===
.org $f157

loc_f157:
	lda $0282                ; $f157: ad 82 02 SWCHB
	lsr                      ; $f15a: 4a
	bcs $f170                ; $f15b: b0 13
loc_f15d:
	lda #$0f                 ; $f15d: a9 0f PF2
	sta $87                  ; $f15f: 85 87
	lda #$ff                 ; $f161: a9 ff
	sta $88                  ; $f163: 85 88
	lda #$80                 ; $f165: a9 80
	sta $dd                  ; $f167: 85 dd
	ldx #$e6                 ; $f169: a2 e6
	jsr $f5bd                ; $f16b: 20 bd f5
loc_f16e:
	beq $f1d0                ; $f16e: f0 60
loc_f170:
	ldy #$02                 ; $f170: a0 02 WSYNC
	lda $dd                  ; $f172: a5 dd
	and $88                  ; $f174: 25 88
	cmp #$f0                 ; $f176: c9 f0
	bcc $f182                ; $f178: 90 08
loc_f17a:
	lda $86                  ; $f17a: a5 86
	and #$30                 ; $f17c: 29 30
	bne $f182                ; $f17e: d0 02
loc_f180:
	ldy #$0e                 ; $f180: a0 0e PF1
loc_f182:
	sty $dc                  ; $f182: 84 dc
	lda $86                  ; $f184: a5 86
	and #$3f                 ; $f186: 29 3f
	bne $f192                ; $f188: d0 08
loc_f18a:
	sta $89                  ; $f18a: 85 89
	inc $dd                  ; $f18c: e6 dd
	bne $f192                ; $f18e: d0 02
loc_f190:
	sta $88                  ; $f190: 85 88
loc_f192:
	lda $0282                ; $f192: ad 82 02 SWCHB
	and #$02                 ; $f195: 29 02 WSYNC
	beq $f19d                ; $f197: f0 04
loc_f199:
	sta $89                  ; $f199: 85 89
	bne $f1f1                ; $f19b: d0 54
loc_f19d:
	bit $89                  ; $f19d: 24 89
	bmi $f1f1                ; $f19f: 30 50
loc_f1a1:
	inc $80                  ; $f1a1: e6 80

; === Code Block $f1a3-$f1f1 ===
.org $f1a3

loc_f1a3:
	ldx #$df                 ; $f1a3: a2 df
	jsr $f5bd                ; $f1a5: 20 bd f5
loc_f1a8:
	lda #$ff                 ; $f1a8: a9 ff
	sta $89                  ; $f1aa: 85 89
	ldy $80                  ; $f1ac: a4 80
	lda $f7d8,y              ; $f1ae: b9 d8 f7
	sta $a3                  ; $f1b1: 85 a3
	eor #$ff                 ; $f1b3: 49 ff
	bne $f1bb                ; $f1b5: d0 04
loc_f1b7:
	ldx #$dd                 ; $f1b7: a2 dd
	bne $f1a5                ; $f1b9: d0 ea
loc_f1bb:
	lda $81                  ; $f1bb: a5 81
	sed                      ; $f1bd: f8
	clc                      ; $f1be: 18
	adc #$01                 ; $f1bf: 69 01 VBLANK
	sta $81                  ; $f1c1: 85 81
	sta $a1                  ; $f1c3: 85 a1
	cld                      ; $f1c5: d8
	bit $a3                  ; $f1c6: 24 a3
	bpl $f1d0                ; $f1c8: 10 06
loc_f1ca:
	inc $85                  ; $f1ca: e6 85
	bvc $f1d0                ; $f1cc: 50 02
loc_f1ce:
	inc $85                  ; $f1ce: e6 85
loc_f1d0:
	jsr $f525                ; $f1d0: 20 25 f5
loc_f1d3:
	lda #$32                 ; $f1d3: a9 32
	sta $a5                  ; $f1d5: 85 a5
	lda #$86                 ; $f1d7: a9 86
	sta $a4                  ; $f1d9: 85 a4
	bit $a3                  ; $f1db: 24 a3
	bmi $f1f1                ; $f1dd: 30 12
loc_f1df:
	sta $a5                  ; $f1df: 85 a5
	sta $11                  ; $f1e1: 85 11 RESP1
	lda #$08                 ; $f1e3: a9 08 COLUPF
	sta $96                  ; $f1e5: 85 96
	lda #$20                 ; $f1e7: a9 20 HMP0
	sta $20                  ; $f1e9: 85 20 HMP0
	sta $21                  ; $f1eb: 85 21 HMP1
	sta $02                  ; $f1ed: 85 02 WSYNC
	sta $2a                  ; $f1ef: 85 2a HMOVE
loc_f1f1:
	rts                      ; $f1f1: 60

; === Code Block $f1f2-$f213 ===
.org $f1f2

loc_f1f2:
	ldx #$01                 ; $f1f2: a2 01 VBLANK
	lda $a1,x                ; $f1f4: b5 a1
	and #$0f                 ; $f1f6: 29 0f PF2
	sta $d2                  ; $f1f8: 85 d2
	asl                      ; $f1fa: 0a
	asl                      ; $f1fb: 0a
	clc                      ; $f1fc: 18
	adc $d2                  ; $f1fd: 65 d2
	sta $e0,x                ; $f1ff: 95 e0
	lda $a1,x                ; $f201: b5 a1
	and #$f0                 ; $f203: 29 f0
	lsr                      ; $f205: 4a
	lsr                      ; $f206: 4a
	sta $d2                  ; $f207: 85 d2
	lsr                      ; $f209: 4a
	lsr                      ; $f20a: 4a
	clc                      ; $f20b: 18
	adc $d2                  ; $f20c: 65 d2
	sta $e2,x                ; $f20e: 95 e2
	dex                      ; $f210: ca
	bpl $f1f4                ; $f211: 10 e1
loc_f213:
	rts                      ; $f213: 60

; === Code Block $f214-$f253 ===
.org $f214

loc_f214:
	bit $83                  ; $f214: 24 83
	bvc $f21c                ; $f216: 50 04
loc_f218:
	lda #$30                 ; $f218: a9 30
	bpl $f21e                ; $f21a: 10 02
loc_f21c:
	lda #$20                 ; $f21c: a9 20 HMP0
loc_f21e:
	sta $b1                  ; $f21e: 85 b1
	ldx #$03                 ; $f220: a2 03 RSYNC
	jsr $f254                ; $f222: 20 54 f2
loc_f225:
	dex                      ; $f225: ca
	jsr $f254                ; $f226: 20 54 f2
loc_f229:
	dex                      ; $f229: ca
	lda $8d,x                ; $f22a: b5 8d
	and #$08                 ; $f22c: 29 08 COLUPF
	lsr                      ; $f22e: 4a
	lsr                      ; $f22f: 4a
	stx $d1                  ; $f230: 86 d1
	clc                      ; $f232: 18
	adc $d1                  ; $f233: 65 d1
	tay                      ; $f235: a8
	lda $00a8,y              ; $f236: b9 a8 00
	sec                      ; $f239: 38
	bmi $f23d                ; $f23a: 30 01
loc_f23c:
	clc                      ; $f23c: 18
loc_f23d:
	rol                      ; $f23d: 2a
	sta $00a8,y              ; $f23e: 99 a8 00
	bcc $f250                ; $f241: 90 0d
loc_f243:
	lda $ac,x                ; $f243: b5 ac
	and #$01                 ; $f245: 29 01 VBLANK
	asl                      ; $f247: 0a
	asl                      ; $f248: 0a
	asl                      ; $f249: 0a
	asl                      ; $f24a: 0a
	sta $b1                  ; $f24b: 85 b1
	jsr $f254                ; $f24d: 20 54 f2
loc_f250:
	dex                      ; $f250: ca
	beq $f22a                ; $f251: f0 d7
loc_f253:
	rts                      ; $f253: 60

; === Code Block $f254-$f2a8 ===
.org $f254

loc_f254:
	inc $ac,x                ; $f254: f6 ac
	lda $95,x                ; $f256: b5 95
	and #$0f                 ; $f258: 29 0f PF2
	clc                      ; $f25a: 18
	adc $b1                  ; $f25b: 65 b1
	tay                      ; $f25d: a8
	lda $f5f7,y              ; $f25e: b9 f7 f5
	sta $b0                  ; $f261: 85 b0
	bit $82                  ; $f263: 24 82
	bvs $f27a                ; $f265: 70 13
loc_f267:
	lda $95,x                ; $f267: b5 95
	sec                      ; $f269: 38
	sbc #$02                 ; $f26a: e9 02 WSYNC
	and #$03                 ; $f26c: 29 03 RSYNC
	bne $f27a                ; $f26e: d0 0a
loc_f270:
	lda $ac,x                ; $f270: b5 ac
	and #$03                 ; $f272: 29 03 RSYNC
	bne $f27a                ; $f274: d0 04
loc_f276:
	lda #$08                 ; $f276: a9 08 COLUPF
	sta $b0                  ; $f278: 85 b0
loc_f27a:
	lda $b0                  ; $f27a: a5 b0
loc_f27c:
	sta $20,x                ; $f27c: 95 20 HMP0
	and #$0f                 ; $f27e: 29 0f PF2
	sec                      ; $f280: 38
	sbc #$08                 ; $f281: e9 08 COLUPF
	sta $d4                  ; $f283: 85 d4
	clc                      ; $f285: 18
	adc $a4,x                ; $f286: 75 a4
	bit $a3                  ; $f288: 24 a3
	bmi $f290                ; $f28a: 30 04
loc_f28c:
	cpx #$02                 ; $f28c: e0 02 WSYNC
	bcs $f2a0                ; $f28e: b0 10
loc_f290:
	cmp #$db                 ; $f290: c9 db
	bcs $f298                ; $f292: b0 04
loc_f294:
	cmp #$25                 ; $f294: c9 25 VDELP0
	bcs $f2a0                ; $f296: b0 08
loc_f298:
	lda #$d9                 ; $f298: a9 d9
	bit $d4                  ; $f29a: 24 d4
	bmi $f2a0                ; $f29c: 30 02
loc_f29e:
	lda #$28                 ; $f29e: a9 28 RESMP0
loc_f2a0:
	sta $a4,x                ; $f2a0: 95 a4
	cpx #$02                 ; $f2a2: e0 02 WSYNC
	bcs $f2a8                ; $f2a4: b0 02
loc_f2a6:
	sta $25,x                ; $f2a6: 95 25 VDELP0
loc_f2a8:
	rts                      ; $f2a8: 60

; === Code Block $f2a9-$f2d9 ===
.org $f2a9

loc_f2a9:
	lda #$01                 ; $f2a9: a9 01 VBLANK
	and $86                  ; $f2ab: 25 86
	tax                      ; $f2ad: aa
	lda $95,x                ; $f2ae: b5 95
	sta $0b,x                ; $f2b0: 95 0b REFP0
	and #$0f                 ; $f2b2: 29 0f PF2
	tay                      ; $f2b4: a8
	bit $83                  ; $f2b5: 24 83
	bpl $f2bb                ; $f2b7: 10 02
loc_f2b9:
	sty $97,x                ; $f2b9: 94 97
loc_f2bb:
	txa                      ; $f2bb: 8a
	eor #$0e                 ; $f2bc: 49 0e PF1
	tax                      ; $f2be: aa
	tya                      ; $f2bf: 98
	asl                      ; $f2c0: 0a
	asl                      ; $f2c1: 0a
	asl                      ; $f2c2: 0a
	cmp #$3f                 ; $f2c3: c9 3f
	clc                      ; $f2c5: 18
	bmi $f2cb                ; $f2c6: 30 03
loc_f2c8:
	sec                      ; $f2c8: 38
	eor #$47                 ; $f2c9: 49 47 COLUP1
loc_f2cb:
	tay                      ; $f2cb: a8
	lda ($bb),y              ; $f2cc: b1 bb
	sta $bd,x                ; $f2ce: 95 bd
	bcc $f2d4                ; $f2d0: 90 02
loc_f2d2:
	dey                      ; $f2d2: 88
	dey                      ; $f2d3: 88
loc_f2d4:
	iny                      ; $f2d4: c8
	dex                      ; $f2d5: ca
	dex                      ; $f2d6: ca
	bpl $f2cc                ; $f2d7: 10 f3
loc_f2d9:
	rts                      ; $f2d9: 60

; === Code Block $f2da-$f30b ===
.org $f2da

loc_f2da:
	lda $8a                  ; $f2da: a5 8a
	sec                      ; $f2dc: 38
	sbc #$02                 ; $f2dd: e9 02 WSYNC
	bcc $f30c                ; $f2df: 90 2b
loc_f2e1:
	sta $8a                  ; $f2e1: 85 8a
	cmp #$02                 ; $f2e3: c9 02 WSYNC
	bcc $f30b                ; $f2e5: 90 24
loc_f2e7:
	and #$01                 ; $f2e7: 29 01 VBLANK
	tax                      ; $f2e9: aa
	inc $95,x                ; $f2ea: f6 95
	lda $d8,x                ; $f2ec: b5 d8
	sta $d6,x                ; $f2ee: 95 d6
	lda $8a                  ; $f2f0: a5 8a
	cmp #$f7                 ; $f2f2: c9 f7
	bcc $f2f9                ; $f2f4: 90 03
loc_f2f6:
	jsr $f508                ; $f2f6: 20 08 f5
loc_f2f9:
	lda $8a                  ; $f2f9: a5 8a
	bpl $f30b                ; $f2fb: 10 0e
loc_f2fd:
	lsr                      ; $f2fd: 4a
	lsr                      ; $f2fe: 4a
	lsr                      ; $f2ff: 4a
	sta $19,x                ; $f300: 95 19 AUDV0
	lda #$08                 ; $f302: a9 08 COLUPF
	sta $15,x                ; $f304: 95 15 AUDC0
	lda $f7fe,x              ; $f306: bd fe f7
	sta $17,x                ; $f309: 95 17 AUDF0
loc_f30b:
	rts                      ; $f30b: 60

; === Code Block $f30c-$f37a ===
.org $f30c

loc_f30c:
	ldx #$01                 ; $f30c: a2 01 VBLANK
	lda $0282                ; $f30e: ad 82 02 SWCHB
	sta $d5                  ; $f311: 85 d5
	lda $0280                ; $f313: ad 80 02 SWCHA
	bit $88                  ; $f316: 24 88
	bmi $f31c                ; $f318: 30 02
loc_f31a:
	lda #$ff                 ; $f31a: a9 ff
loc_f31c:
	eor #$ff                 ; $f31c: 49 ff
	and #$0f                 ; $f31e: 29 0f PF2
	sta $d2                  ; $f320: 85 d2
	ldy $85                  ; $f322: a4 85
	lda $f70f,y              ; $f324: b9 0f f7
	clc                      ; $f327: 18
	adc $d2                  ; $f328: 65 d2
	tay                      ; $f32a: a8
	lda $f712,y              ; $f32b: b9 12 f7
	and #$0f                 ; $f32e: 29 0f PF2
	sta $d1                  ; $f330: 85 d1
	beq $f338                ; $f332: f0 04
loc_f334:
	cmp $91,x                ; $f334: d5 91
	bne $f33c                ; $f336: d0 04
loc_f338:
	dec $93,x                ; $f338: d6 93
	bne $f349                ; $f33a: d0 0d
loc_f33c:
	sta $91,x                ; $f33c: 95 91
	lda #$0f                 ; $f33e: a9 0f PF2
	sta $93,x                ; $f340: 95 93
	lda $d1                  ; $f342: a5 d1
	clc                      ; $f344: 18
	adc $95,x                ; $f345: 75 95
	sta $95,x                ; $f347: 95 95
loc_f349:
	inc $8d,x                ; $f349: f6 8d
	bmi $f36b                ; $f34b: 30 1e
loc_f34d:
	lda $f712,y              ; $f34d: b9 12 f7
	lsr                      ; $f350: 4a
	lsr                      ; $f351: 4a
	lsr                      ; $f352: 4a
	lsr                      ; $f353: 4a
	bit $d5                  ; $f354: 24 d5
	bmi $f37b                ; $f356: 30 23
loc_f358:
	sta $8b,x                ; $f358: 95 8b
	asl                      ; $f35a: 0a
	tay                      ; $f35b: a8
	lda $f637,y              ; $f35c: b9 37 f6
	sta $a8,x                ; $f35f: 95 a8
	iny                      ; $f361: c8
	lda $f637,y              ; $f362: b9 37 f6
	sta $aa,x                ; $f365: 95 aa
	lda #$f0                 ; $f367: a9 f0
	sta $8d,x                ; $f369: 95 8d
loc_f36b:
	jsr $f380                ; $f36b: 20 80 f3
loc_f36e:
	lda $0280                ; $f36e: ad 80 02 SWCHA
	lsr                      ; $f371: 4a
	lsr                      ; $f372: 4a
	lsr                      ; $f373: 4a
	lsr                      ; $f374: 4a
	asl $d5                  ; $f375: 06 d5
	dex                      ; $f377: ca
	beq $f316                ; $f378: f0 9c
loc_f37a:
	rts                      ; $f37a: 60

; === Code Block $f37b-$f3b6 ===
.org $f37b

loc_f37b:
	sec                      ; $f37b: 38
	sbc $85                  ; $f37c: e5 85
	bpl $f358                ; $f37e: 10 d8
loc_f380:
	lda $a3                  ; $f380: a5 a3
	bmi $f38c                ; $f382: 30 08
loc_f384:
	and #$01                 ; $f384: 29 01 VBLANK
	beq $f38c                ; $f386: f0 04
loc_f388:
	lda $db                  ; $f388: a5 db
	sta $d6,x                ; $f38a: 95 d6
loc_f38c:
	lda $99,x                ; $f38c: b5 99
	beq $f3b7                ; $f38e: f0 27
loc_f390:
	lda $d8,x                ; $f390: b5 d8
	sta $d6,x                ; $f392: 95 d6
	lda $99,x                ; $f394: b5 99
	cmp #$07                 ; $f396: c9 07 COLUP1
	bcc $f3ae                ; $f398: 90 14
loc_f39a:
	bit $d5                  ; $f39a: 24 d5
	bpl $f3a2                ; $f39c: 10 04
loc_f39e:
	cmp #$1c                 ; $f39e: c9 1c GRP1
	bcc $f3ae                ; $f3a0: 90 0c
loc_f3a2:
	cmp #$30                 ; $f3a2: c9 30
	bcc $f3c5                ; $f3a4: 90 1f
loc_f3a6:
	cmp #$37                 ; $f3a6: c9 37
	bcs $f3cb                ; $f3a8: b0 21
loc_f3aa:
	bit $83                  ; $f3aa: 24 83
	bvc $f3cb                ; $f3ac: 50 1d
loc_f3ae:
	lda #$00                 ; $f3ae: a9 00 VSYNC
	sta $99,x                ; $f3b0: 95 99
	lda #$ff                 ; $f3b2: a9 ff
	sta $28,x                ; $f3b4: 95 28 RESMP0
	rts                      ; $f3b6: 60

; === Code Block $f3b7-$f3c2 ===
.org $f3b7

loc_f3b7:
	bit $88                  ; $f3b7: 24 88
	bpl $f3bf                ; $f3b9: 10 04
loc_f3bb:
	lda $3c,x                ; $f3bb: b5 3c
	bpl $f3f6                ; $f3bd: 10 37
loc_f3bf:
	jsr $f410                ; $f3bf: 20 10 f4
loc_f3c2:
	jmp $f3ae                ; $f3c2: 4c ae f3

; === Code Block $f3c5-$f3c8 ===
.org $f3c5

loc_f3c5:
	jsr $f410                ; $f3c5: 20 10 f4
loc_f3c8:
	jmp $f3de                ; $f3c8: 4c de f3

; === Code Block $f3cb-$f3d6 ===
.org $f3cb

loc_f3cb:
	lda $9f,x                ; $f3cb: b5 9f
	beq $f3d9                ; $f3cd: f0 0a
loc_f3cf:
	jsr $f410                ; $f3cf: 20 10 f4
loc_f3d2:
	lda #$30                 ; $f3d2: a9 30
	sta $99,x                ; $f3d4: 95 99
	jmp $f3de                ; $f3d6: 4c de f3

; === Code Block $f3d9-$f3de ===
.org $f3d9

loc_f3d9:
	lda $99,x                ; $f3d9: b5 99
	jsr $f300                ; $f3db: 20 00 f3

; === Code Block $f3de-$f3f6 ===
.org $f3de

loc_f3de:
	lda $86                  ; $f3de: a5 86
	and #$03                 ; $f3e0: 29 03 RSYNC
	beq $f3f0                ; $f3e2: f0 0c
loc_f3e4:
	bit $84                  ; $f3e4: 24 84
	bvs $f3f2                ; $f3e6: 70 0a
loc_f3e8:
	bit $82                  ; $f3e8: 24 82
	bvc $f3f0                ; $f3ea: 50 04
loc_f3ec:
	and #$01                 ; $f3ec: 29 01 VBLANK
	bne $f3f2                ; $f3ee: d0 02
loc_f3f0:
	dec $99,x                ; $f3f0: d6 99
loc_f3f2:
	lda #$00                 ; $f3f2: a9 00 VSYNC
	beq $f3b4                ; $f3f4: f0 be

; === Code Block $f3f6-$f40d ===
.org $f3f6

loc_f3f6:
	lda #$3f                 ; $f3f6: a9 3f
	sta $99,x                ; $f3f8: 95 99
	sec                      ; $f3fa: 38
	lda $a4,x                ; $f3fb: b5 a4
	sbc #$06                 ; $f3fd: e9 06 COLUP0
	sta $a6,x                ; $f3ff: 95 a6
	lda $95,x                ; $f401: b5 95
	sta $97,x                ; $f403: 95 97
	lda #$1f                 ; $f405: a9 1f ENABL
	sta $9b,x                ; $f407: 95 9b
	lda #$00                 ; $f409: a9 00 VSYNC
	sta $9d,x                ; $f40b: 95 9d
	jmp $f3cb                ; $f40d: 4c cb f3

; === Code Block $f410-$f420 ===
.org $f410

loc_f410:
	lda $9f,x                ; $f410: b5 9f
	beq $f421                ; $f412: f0 0d
loc_f414:
	lda #$04                 ; $f414: a9 04 NUSIZ0
	sta $15,x                ; $f416: 95 15 AUDC0
	lda #$07                 ; $f418: a9 07 COLUP1
	sta $19,x                ; $f41a: 95 19 AUDV0
	lda $9b,x                ; $f41c: b5 9b
	sta $17,x                ; $f41e: 95 17 AUDF0
	rts                      ; $f420: 60

; === Code Block $f421-$f443 ===
.org $f421

loc_f421:
	ldy $85                  ; $f421: a4 85
	lda $f733,y              ; $f423: b9 33 f7
	and $88                  ; $f426: 25 88
	sta $19,x                ; $f428: 95 19 AUDV0
	lda $f736,y              ; $f42a: b9 36 f7
	sta $15,x                ; $f42d: 95 15 AUDC0
	clc                      ; $f42f: 18
	lda #$00                 ; $f430: a9 00 VSYNC
	dey                      ; $f432: 88
	bmi $f439                ; $f433: 30 04
loc_f435:
	adc #$0c                 ; $f435: 69 0c REFP1
	bpl $f432                ; $f437: 10 f9
loc_f439:
	adc $8b,x                ; $f439: 75 8b
	tay                      ; $f43b: a8
	txa                      ; $f43c: 8a
	asl                      ; $f43d: 0a
	adc $f739,y              ; $f43e: 79 39 f7
	sta $17,x                ; $f441: 95 17 AUDF0
	rts                      ; $f443: 60

; === Code Block $f444-$f475 ===
.org $f444

loc_f444:
	ldx #$01                 ; $f444: a2 01 VBLANK
	lda $30,x                ; $f446: b5 30
	bpl $f476                ; $f448: 10 2c
loc_f44a:
	bit $84                  ; $f44a: 24 84
	bvc $f454                ; $f44c: 50 06
loc_f44e:
	lda $9b,x                ; $f44e: b5 9b
	cmp #$1f                 ; $f450: c9 1f ENABL
	beq $f476                ; $f452: f0 22
loc_f454:
	inc $95,x                ; $f454: f6 95
	inc $97,x                ; $f456: f6 97
	sed                      ; $f458: f8
	lda $a1,x                ; $f459: b5 a1
	clc                      ; $f45b: 18
	adc #$01                 ; $f45c: 69 01 VBLANK
	sta $a1,x                ; $f45e: 95 a1
	cld                      ; $f460: d8
	txa                      ; $f461: 8a
	clc                      ; $f462: 18
	adc #$fd                 ; $f463: 69 fd
	sta $8a                  ; $f465: 85 8a
	lda #$ff                 ; $f467: a9 ff
	sta $28                  ; $f469: 85 28 RESMP0
	sta $29                  ; $f46b: 85 29 RESMP1
	lda #$00                 ; $f46d: a9 00 VSYNC
	sta $19,x                ; $f46f: 95 19 AUDV0
	sta $99                  ; $f471: 85 99
	sta $9a                  ; $f473: 85 9a
	rts                      ; $f475: 60

; === Code Block $f476-$f47a ===
.org $f476

loc_f476:
	bit $a3                  ; $f476: 24 a3
	bpl $f47d                ; $f478: 10 03
loc_f47a:
	jmp $f501                ; $f47a: 4c 01 f5

; === Code Block $f47d-$f493 ===
.org $f47d

loc_f47d:
	lda $9f,x                ; $f47d: b5 9f
	beq $f48b                ; $f47f: f0 0a
loc_f481:
	cmp #$04                 ; $f481: c9 04 NUSIZ0
	inc $9f,x                ; $f483: f6 9f
	bcc $f48b                ; $f485: 90 04
loc_f487:
	lda #$00                 ; $f487: a9 00 VSYNC
	sta $9f,x                ; $f489: 95 9f
loc_f48b:
	lda $34,x                ; $f48b: b5 34
	bmi $f496                ; $f48d: 30 07
loc_f48f:
	lda #$00                 ; $f48f: a9 00 VSYNC
	sta $9d,x                ; $f491: 95 9d
	jmp $f4d6                ; $f493: 4c d6 f4

; === Code Block $f496-$f4b4 ===
.org $f496

loc_f496:
	bit $82                  ; $f496: 24 82
	bvc $f4d0                ; $f498: 50 36
loc_f49a:
	lda $9d,x                ; $f49a: b5 9d
	bne $f4b7                ; $f49c: d0 19
loc_f49e:
	inc $9f,x                ; $f49e: f6 9f
	dec $9b,x                ; $f4a0: d6 9b
	lda $97,x                ; $f4a2: b5 97
	sta $b2,x                ; $f4a4: 95 b2
	eor #$ff                 ; $f4a6: 49 ff
	sta $97,x                ; $f4a8: 95 97
	inc $97,x                ; $f4aa: f6 97
	lda $97,x                ; $f4ac: b5 97
	and #$03                 ; $f4ae: 29 03 RSYNC
	bne $f4b4                ; $f4b0: d0 02
loc_f4b2:
	inc $97,x                ; $f4b2: f6 97
loc_f4b4:
	jmp $f4d4                ; $f4b4: 4c d4 f4

; === Code Block $f4b7-$f4c3 ===
.org $f4b7

loc_f4b7:
	cmp #$01                 ; $f4b7: c9 01 VBLANK
	beq $f4c6                ; $f4b9: f0 0b
loc_f4bb:
	cmp #$03                 ; $f4bb: c9 03 RSYNC
	bcc $f4d4                ; $f4bd: 90 15
loc_f4bf:
	bne $f4d4                ; $f4bf: d0 13
loc_f4c1:
	lda $b2,x                ; $f4c1: b5 b2
	jmp $f4c8                ; $f4c3: 4c c8 f4

; === Code Block $f4c6-$f4cd ===
.org $f4c6

loc_f4c6:
	lda $97,x                ; $f4c6: b5 97
loc_f4c8:
	clc                      ; $f4c8: 18
	adc #$08                 ; $f4c9: 69 08 COLUPF
	sta $97,x                ; $f4cb: 95 97
	jmp $f4d4                ; $f4cd: 4c d4 f4

; === Code Block $f4d0-$f4d6 ===
.org $f4d0

loc_f4d0:
	lda #$01                 ; $f4d0: a9 01 VBLANK
	sta $99,x                ; $f4d2: 95 99
loc_f4d4:
	inc $9d,x                ; $f4d4: f6 9d

; === Code Block $f4d6-$f501 ===
.org $f4d6

loc_f4d6:
	lda $32,x                ; $f4d6: b5 32
	bmi $f4de                ; $f4d8: 30 04
loc_f4da:
	lda $37                  ; $f4da: a5 37
	bpl $f4e7                ; $f4dc: 10 09
loc_f4de:
	lda $8a                  ; $f4de: a5 8a
	cmp #$02                 ; $f4e0: c9 02 WSYNC
	bcc $f4ed                ; $f4e2: 90 09
loc_f4e4:
	jsr $f508                ; $f4e4: 20 08 f5
loc_f4e7:
	lda #$03                 ; $f4e7: a9 03 RSYNC
	sta $e4,x                ; $f4e9: 95 e4
	bne $f501                ; $f4eb: d0 14
loc_f4ed:
	dec $e4,x                ; $f4ed: d6 e4
	bmi $f4f7                ; $f4ef: 30 06
loc_f4f1:
	lda $8b,x                ; $f4f1: b5 8b
	beq $f501                ; $f4f3: f0 0c
loc_f4f5:
	bne $f4f9                ; $f4f5: d0 02
loc_f4f7:
	inc $95,x                ; $f4f7: f6 95
loc_f4f9:
	lda $95,x                ; $f4f9: b5 95
	clc                      ; $f4fb: 18
	adc #$08                 ; $f4fc: 69 08 COLUPF
	jsr $f50f                ; $f4fe: 20 0f f5

; === Code Block $f501-$f504 ===
.org $f501

loc_f501:
	dex                      ; $f501: ca
	bmi $f507                ; $f502: 30 03
loc_f504:
	jmp $f446                ; $f504: 4c 46 f4

; === Code Block $f507-$f507 ===
.org $f507

loc_f507:
	rts                      ; $f507: 60

; === Code Block $f508-$f524 ===
.org $f508

loc_f508:
	txa                      ; $f508: 8a
	eor #$01                 ; $f509: 49 01 VBLANK
	tay                      ; $f50b: a8
	lda $0097,y              ; $f50c: b9 97 00
	and #$0f                 ; $f50f: 29 0f PF2
	tay                      ; $f511: a8
	lda $f627,y              ; $f512: b9 27 f6
	jsr $f27c                ; $f515: 20 7c f2
loc_f518:
	lda #$00                 ; $f518: a9 00 VSYNC
	sta $a8,x                ; $f51a: 95 a8
	sta $aa,x                ; $f51c: 95 aa
	sta $8d,x                ; $f51e: 95 8d
	lda $d8,x                ; $f520: b5 d8
	sta $d6,x                ; $f522: 95 d6
	rts                      ; $f524: 60

; === Code Block $f525-$f571 ===
.org $f525

loc_f525:
	ldx $85                  ; $f525: a6 85
	lda $f7c6,x              ; $f527: bd c6 f7
	sta $bb                  ; $f52a: 85 bb
	lda $f7c9,x              ; $f52c: bd c9 f7
	sta $bc                  ; $f52f: 85 bc
	lda $a3                  ; $f531: a5 a3
	lsr                      ; $f533: 4a
	lsr                      ; $f534: 4a
	and #$03                 ; $f535: 29 03 RSYNC
	tax                      ; $f537: aa
	lda $a3                  ; $f538: a5 a3
	bpl $f546                ; $f53a: 10 0a
loc_f53c:
	and #$08                 ; $f53c: 29 08 COLUPF
	beq $f544                ; $f53e: f0 04
loc_f540:
	ldx #$03                 ; $f540: a2 03 RSYNC
	bpl $f548                ; $f542: 10 04
loc_f544:
	lda #$80                 ; $f544: a9 80
loc_f546:
	sta $82                  ; $f546: 85 82
loc_f548:
	lda $a3                  ; $f548: a5 a3
	asl                      ; $f54a: 0a
	asl                      ; $f54b: 0a
	bit $a3                  ; $f54c: 24 a3
	bmi $f556                ; $f54e: 30 06
loc_f550:
	sta $02                  ; $f550: 85 02 WSYNC
	sta $84                  ; $f552: 85 84
	and #$80                 ; $f554: 29 80
loc_f556:
	sta $83                  ; $f556: 85 83
	lda #$f7                 ; $f558: a9 f7
	sta $b6                  ; $f55a: 85 b6
	sta $b8                  ; $f55c: 85 b8
	sta $ba                  ; $f55e: 85 ba
	lda $f7cc,x              ; $f560: bd cc f7
	sta $10                  ; $f563: 85 10 RESP0
	sta $b5                  ; $f565: 85 b5
	lda $f7d0,x              ; $f567: bd d0 f7
	sta $b7                  ; $f56a: 85 b7
	lda $f7d4,x              ; $f56c: bd d4 f7
	sta $b9                  ; $f56f: 85 b9
	rts                      ; $f571: 60

; === Code Block $f572-$f5bc ===
.org $f572

loc_f572:
	lda $a3                  ; $f572: a5 a3
	and #$87                 ; $f574: 29 87
	bmi $f57a                ; $f576: 30 02
loc_f578:
	lda #$00                 ; $f578: a9 00 VSYNC
loc_f57a:
	asl                      ; $f57a: 0a
	tax                      ; $f57b: aa
	lda $f75d,x              ; $f57c: bd 5d f7
	sta $04                  ; $f57f: 85 04 NUSIZ0
	lda $f75e,x              ; $f581: bd 5e f7
	sta $05                  ; $f584: 85 05 NUSIZ1
	lda $a3                  ; $f586: a5 a3
	and #$c0                 ; $f588: 29 c0
	lsr                      ; $f58a: 4a
	lsr                      ; $f58b: 4a
	lsr                      ; $f58c: 4a
	lsr                      ; $f58d: 4a
	tay                      ; $f58e: a8
	lda $88                  ; $f58f: a5 88
	sta $0282                ; $f591: 8d 82 02 SWCHB
	eor #$ff                 ; $f594: 49 ff
	and $dd                  ; $f596: 25 dd
	sta $d1                  ; $f598: 85 d1
	ldx #$ff                 ; $f59a: a2 ff
	lda $0282                ; $f59c: ad 82 02 SWCHB
	and #$08                 ; $f59f: 29 08 COLUPF
	bne $f5a7                ; $f5a1: d0 04
loc_f5a3:
	ldy #$10                 ; $f5a3: a0 10 RESP0
	ldx #$0f                 ; $f5a5: a2 0f PF2
loc_f5a7:
	stx $d2                  ; $f5a7: 86 d2
	ldx #$03                 ; $f5a9: a2 03 RSYNC
	lda $f765,y              ; $f5ab: b9 65 f7
	eor $d1                  ; $f5ae: 45 d1
	and $d2                  ; $f5b0: 25 d2
	sta $06,x                ; $f5b2: 95 06 COLUP0
	sta $d6,x                ; $f5b4: 95 d6
	sta $d8,x                ; $f5b6: 95 d8
	iny                      ; $f5b8: c8
	dex                      ; $f5b9: ca
	bpl $f5ab                ; $f5ba: 10 ef
loc_f5bc:
	rts                      ; $f5bc: 60

; === Code Block $f5bd-$f5c4 ===
.org $f5bd

loc_f5bd:
	lda #$00                 ; $f5bd: a9 00 VSYNC
	inx                      ; $f5bf: e8
	sta $a2,x                ; $f5c0: 95 a2
	bne $f5bf                ; $f5c2: d0 fb
loc_f5c4:
	rts                      ; $f5c4: 60

; === Vectors ===
.org $fffc
	.word reset
	.word reset
