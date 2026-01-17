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
PF1              = $000e
PF2              = $000f
RESP0            = $0010
RESP1            = $0011
RESBL            = $0014
AUDC0            = $0015
AUDC1            = $0016
AUDF0            = $0017
AUDF1            = $0018
AUDV0            = $0019
AUDV1            = $001a
GRP0             = $001b
GRP1             = $001c
ENAM1            = $001e
ENABL            = $001f
HMP0             = $0020
HMP1             = $0021
HMM0             = $0022
HMBL             = $0024
RESMP0           = $0028
HMOVE            = $002a
HMCLR            = $002b
CXCLR            = $002c
VSYNC            = $0040
RESP0            = $0050
GRP1             = $005c
HMP0             = $0060
SWCHA            = $0280
SWCHB            = $0282
INTIM            = $0284
TIM64T           = $0296

; === Code Block $f000-$f147 ===
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
	jsr $f5b0                ; $f00c: 20 b0 f5
loc_f00f:
	ldx #$05                 ; $f00f: a2 05 NUSIZ1
	lda $f6f0,x              ; $f011: bd f0 f6
	eor $86                  ; $f014: 45 86
	and $87                  ; $f016: 25 87
	sta $88,x                ; $f018: 95 88
	cpx #$04                 ; $f01a: e0 04 NUSIZ0
	bcs $f020                ; $f01c: b0 02
loc_f01e:
	sta $06,x                ; $f01e: 95 06 COLUP0
loc_f020:
	dex                      ; $f020: ca
	bpl $f011                ; $f021: 10 ee
loc_f023:
	stx $90                  ; $f023: 86 90
	stx $91                  ; $f025: 86 91
	sta $02                  ; $f027: 85 02 WSYNC
	sta $14                  ; $f029: 85 14 RESBL
	lda #$22                 ; $f02b: a9 22 HMM0
	sta $24                  ; $f02d: 85 24 HMBL
	sta $1f                  ; $f02f: 85 1f ENABL
	lda #$28                 ; $f031: a9 28 RESMP0
	inx                      ; $f033: e8
	stx $08                  ; $f034: 86 08 COLUPF
	jsr $f617                ; $f036: 20 17 f6
loc_f039:
	lda #$30                 ; $f039: a9 30
	sta $0a                  ; $f03b: 85 0a CTRLPF
	inx                      ; $f03d: e8
	jsr $f617                ; $f03e: 20 17 f6
loc_f041:
	lda #$04                 ; $f041: a9 04 NUSIZ0
	sta $04                  ; $f043: 85 04 NUSIZ0
	sta $05                  ; $f045: 85 05 NUSIZ1
	lda $88                  ; $f047: a5 88
	ldy $e6                  ; $f049: a4 e6
	bne $f05d                ; $f04b: d0 10
loc_f04d:
	ldy $e9                  ; $f04d: a4 e9
	cpy #$20                 ; $f04f: c0 20 HMP0
	bcc $f055                ; $f051: 90 02
loc_f053:
	inc $e6                  ; $f053: e6 e6
loc_f055:
	cpy #$1e                 ; $f055: c0 1e ENAM1
	bcc $f05d                ; $f057: 90 04
loc_f059:
	lda $81                  ; $f059: a5 81
	and $87                  ; $f05b: 25 87
loc_f05d:
	sta $06                  ; $f05d: 85 06 COLUP0
	sta $07                  ; $f05f: 85 07 COLUP1
	lda $0284                ; $f061: ad 84 02 INTIM
	bne $f061                ; $f064: d0 fb
loc_f066:
	sta $02                  ; $f066: 85 02 WSYNC
	sta $2a                  ; $f068: 85 2a HMOVE
	sta $01                  ; $f06a: 85 01 VBLANK
	sta $2c                  ; $f06c: 85 2c CXCLR
	ldy #$07                 ; $f06e: a0 07 COLUP1
	sta $02                  ; $f070: 85 02 WSYNC
	sta $2b                  ; $f072: 85 2b HMCLR
	lda ($dd),y              ; $f074: b1 dd
	sta $1b                  ; $f076: 85 1b GRP0
	lda ($e1),y              ; $f078: b1 e1
	sta $1c                  ; $f07a: 85 1c GRP1
	jsr $f613                ; $f07c: 20 13 f6
loc_f07f:
	lda ($df),y              ; $f07f: b1 df
	sta $1b                  ; $f081: 85 1b GRP0
	lda ($e3),y              ; $f083: b1 e3
	sta $1c                  ; $f085: 85 1c GRP1
	dey                      ; $f087: 88
	bpl $f070                ; $f088: 10 e6
loc_f08a:
	lda #$40                 ; $f08a: a9 40 VSYNC
	sta $21                  ; $f08c: 85 21 HMP1
	sta $02                  ; $f08e: 85 02 WSYNC
	sta $2a                  ; $f090: 85 2a HMOVE
	iny                      ; $f092: c8
	sty $1b                  ; $f093: 84 1b GRP0
	sty $1c                  ; $f095: 84 1c GRP1
	lda #$08                 ; $f097: a9 08 COLUPF
	sta $0b                  ; $f099: 85 0b REFP0
	lda $c0                  ; $f09b: a5 c0
	sta $d9                  ; $f09d: 85 d9
	lda $cc                  ; $f09f: a5 cc
	sta $db                  ; $f0a1: 85 db
	ldy #$09                 ; $f0a3: a0 09 COLUBK
	sta $2b                  ; $f0a5: 85 2b HMCLR
	sta $02                  ; $f0a7: 85 02 WSYNC
	sta $2a                  ; $f0a9: 85 2a HMOVE
	lda $8c                  ; $f0ab: a5 8c
	sta $09                  ; $f0ad: 85 09 COLUBK
	lda $89                  ; $f0af: a5 89
	sta $07                  ; $f0b1: 85 07 COLUP1
	sta $02                  ; $f0b3: 85 02 WSYNC
	lda $8d                  ; $f0b5: a5 8d
	cpy #$01                 ; $f0b7: c0 01 VBLANK
	bne $f0bd                ; $f0b9: d0 02
loc_f0bb:
	lda $8c                  ; $f0bb: a5 8c
loc_f0bd:
	sta $09                  ; $f0bd: 85 09 COLUBK
	lda ($d9),y              ; $f0bf: b1 d9
	sta $1c                  ; $f0c1: 85 1c GRP1
	jsr $f615                ; $f0c3: 20 15 f6
loc_f0c6:
	lda ($db),y              ; $f0c6: b1 db
	sta $1c                  ; $f0c8: 85 1c GRP1
	dey                      ; $f0ca: 88
	bne $f0b3                ; $f0cb: d0 e6
loc_f0cd:
	sta $02                  ; $f0cd: 85 02 WSYNC
	sta $2a                  ; $f0cf: 85 2a HMOVE
	lda $8b                  ; $f0d1: a5 8b
	sta $09                  ; $f0d3: 85 09 COLUBK
	lda #$09                 ; $f0d5: a9 09 COLUBK
	sta $95                  ; $f0d7: 85 95
	lda ($d9),y              ; $f0d9: b1 d9
	sta $1c                  ; $f0db: 85 1c GRP1
	nop                      ; $f0dd: ea
	nop                      ; $f0de: ea
	nop                      ; $f0df: ea
	nop                      ; $f0e0: ea
	nop                      ; $f0e1: ea
	lda ($db),y              ; $f0e2: b1 db
	sta $1c                  ; $f0e4: 85 1c GRP1
	ldx $95                  ; $f0e6: a6 95
	lda $b6,x                ; $f0e8: b5 b6
	sta $d9                  ; $f0ea: 85 d9
	lda $c2,x                ; $f0ec: b5 c2
	sta $db                  ; $f0ee: 85 db
	ldy #$0f                 ; $f0f0: a0 0f PF2
	lda #$00                 ; $f0f2: a9 00 VSYNC
	sta $02                  ; $f0f4: 85 02 WSYNC
	sta $2a                  ; $f0f6: 85 2a HMOVE
	sta $0e                  ; $f0f8: 85 0e PF1
	sta $0f                  ; $f0fa: 85 0f PF2
	sta $08                  ; $f0fc: 85 08 COLUPF
	lda ($d9),y              ; $f0fe: b1 d9
	sta $1c                  ; $f100: 85 1c GRP1
	lda $cd,x                ; $f102: b5 cd
	sta $94                  ; $f104: 85 94
	lda $ab,x                ; $f106: b5 ab
	and #$0f                 ; $f108: 29 0f PF2
	sta $f6                  ; $f10a: 85 f6
	lda ($db),y              ; $f10c: b1 db
	dey                      ; $f10e: 88
	sta $1c                  ; $f10f: 85 1c GRP1
	lda $97,x                ; $f111: b5 97
	and #$07                 ; $f113: 29 07 COLUP1
	sta $04                  ; $f115: 85 04 NUSIZ0
	cmp #$05                 ; $f117: c9 05 NUSIZ1
	bne $f11f                ; $f119: d0 04
loc_f11b:
	lda #$c8                 ; $f11b: a9 c8
	bne $f122                ; $f11d: d0 03
loc_f11f:
	lda #$bd                 ; $f11f: a9 bd
	nop                      ; $f121: ea
loc_f122:
	sta $d7                  ; $f122: 85 d7
	lda ($d9),y              ; $f124: b1 d9
	sta $1c                  ; $f126: 85 1c GRP1
	lda $97,x                ; $f128: b5 97
	bmi $f14a                ; $f12a: 30 1e
loc_f12c:
	ldx $f6                  ; $f12c: a6 f6
	cpx #$03                 ; $f12e: e0 03 RSYNC
	lda ($db),y              ; $f130: b1 db
	dex                      ; $f132: ca
	bpl $f132                ; $f133: 10 fd
loc_f135:
	sta $10                  ; $f135: 85 10 RESP0
	bcs $f13c                ; $f137: b0 03
loc_f139:
	jsr $f616                ; $f139: 20 16 f6
loc_f13c:
	dey                      ; $f13c: 88
	sta $1c                  ; $f13d: 85 1c GRP1
	ldx $95                  ; $f13f: a6 95
	lda $ab,x                ; $f141: b5 ab
	sta $20                  ; $f143: 85 20 HMP0
	lda $8c                  ; $f145: a5 8c
	jmp $f166                ; $f147: 4c 66 f1

; === Code Block $f14a-$f196 ===
.org $f14a

loc_f14a:
	nop                      ; $f14a: ea
	nop                      ; $f14b: ea
	sta $2c                  ; $f14c: 85 2c CXCLR
	ldx $95                  ; $f14e: a6 95
	lda $ab,x                ; $f150: b5 ab
	sta $20                  ; $f152: 85 20 HMP0
	lda $f6                  ; $f154: a5 f6
	sec                      ; $f156: 38
	sbc #$06                 ; $f157: e9 06 COLUP0
	tax                      ; $f159: aa
	lda ($db),y              ; $f15a: b1 db
	dey                      ; $f15c: 88
	sta $1c                  ; $f15d: 85 1c GRP1
	lda $8c                  ; $f15f: a5 8c
	dex                      ; $f161: ca
	bpl $f161                ; $f162: 10 fd
loc_f164:
	sta $10                  ; $f164: 85 10 RESP0
loc_f166:
	sta $02                  ; $f166: 85 02 WSYNC
	sta $2a                  ; $f168: 85 2a HMOVE
	sta $06                  ; $f16a: 85 06 COLUP0
	lda ($d7),y              ; $f16c: b1 d7
	sta $1b                  ; $f16e: 85 1b GRP0
	lda $93                  ; $f170: a5 93
	ora $07                  ; $f172: 05 07 COLUP1
	sta $93                  ; $f174: 85 93
	sta $2c                  ; $f176: 85 2c CXCLR
	lda ($d9),y              ; $f178: b1 d9
	sta $1c                  ; $f17a: 85 1c GRP1
	cpy #$06                 ; $f17c: c0 06 COLUP0
	lda $92                  ; $f17e: a5 92
	ora $07                  ; $f180: 05 07 COLUP1
	sta $92                  ; $f182: 85 92
	sta $2c                  ; $f184: 85 2c CXCLR
	lda ($db),y              ; $f186: b1 db
	sta $1c                  ; $f188: 85 1c GRP1
	bcc $f199                ; $f18a: 90 0d
loc_f18c:
	dey                      ; $f18c: 88
	sta $002b                ; $f18d: 8d 2b 00 HMCLR
	lda $94                  ; $f190: a5 94
	eor $86                  ; $f192: 45 86
	and $87                  ; $f194: 25 87
	jmp $f168                ; $f196: 4c 68 f1

; === Code Block $f199-$f217 ===
.org $f199

loc_f199:
	lda $8c                  ; $f199: a5 8c
	dey                      ; $f19b: 88
	sta $02                  ; $f19c: 85 02 WSYNC
	sta $2a                  ; $f19e: 85 2a HMOVE
	sta $06                  ; $f1a0: 85 06 COLUP0
	lda ($d7),y              ; $f1a2: b1 d7
	sta $1b                  ; $f1a4: 85 1b GRP0
	lda $93                  ; $f1a6: a5 93
	ora $07                  ; $f1a8: 05 07 COLUP1
	sta $93                  ; $f1aa: 85 93
	sta $2c                  ; $f1ac: 85 2c CXCLR
	lda ($d9),y              ; $f1ae: b1 d9
	sta $1c                  ; $f1b0: 85 1c GRP1
	nop                      ; $f1b2: ea
	lda $92                  ; $f1b3: a5 92
	ora $07                  ; $f1b5: 05 07 COLUP1
	sta $92                  ; $f1b7: 85 92
	sta $2c                  ; $f1b9: 85 2c CXCLR
	lda ($db),y              ; $f1bb: b1 db
	sta $1c                  ; $f1bd: 85 1c GRP1
	dey                      ; $f1bf: 88
	sta $02                  ; $f1c0: 85 02 WSYNC
	sta $2a                  ; $f1c2: 85 2a HMOVE
	lda #$00                 ; $f1c4: a9 00 VSYNC
	sta $1b                  ; $f1c6: 85 1b GRP0
	lda ($d9),y              ; $f1c8: b1 d9
	sta $1c                  ; $f1ca: 85 1c GRP1
	ldx $95                  ; $f1cc: a6 95
	bit $92                  ; $f1ce: 24 92
	bpl $f1d4                ; $f1d0: 10 02
loc_f1d2:
	stx $90                  ; $f1d2: 86 90
loc_f1d4:
	lda $93                  ; $f1d4: a5 93
	ora $07                  ; $f1d6: 05 07 COLUP1
	bpl $f1dc                ; $f1d8: 10 02
loc_f1da:
	stx $91                  ; $f1da: 86 91
loc_f1dc:
	lda ($db),y              ; $f1dc: b1 db
	sta $1c                  ; $f1de: 85 1c GRP1
	sta $2c                  ; $f1e0: 85 2c CXCLR
	dey                      ; $f1e2: 88
	lda $95                  ; $f1e3: a5 95
	beq $f252                ; $f1e5: f0 6b
loc_f1e7:
	ldx $8b                  ; $f1e7: a6 8b
	cmp #$05                 ; $f1e9: c9 05 NUSIZ1
	bne $f1ef                ; $f1eb: d0 02
loc_f1ed:
	ldx $89                  ; $f1ed: a6 89
loc_f1ef:
	sta $02                  ; $f1ef: 85 02 WSYNC
	sta $2a                  ; $f1f1: 85 2a HMOVE
	lda #$aa                 ; $f1f3: a9 aa
	sta $0d                  ; $f1f5: 85 0d PF0
	sta $0f                  ; $f1f7: 85 0f PF2
	lsr                      ; $f1f9: 4a
	sta $0e                  ; $f1fa: 85 0e PF1
	stx $08                  ; $f1fc: 86 08 COLUPF
	lda ($d9),y              ; $f1fe: b1 d9
	sta $1c                  ; $f200: 85 1c GRP1
	dec $95                  ; $f202: c6 95
	lda ($db),y              ; $f204: b1 db
	sta $1c                  ; $f206: 85 1c GRP1
	dey                      ; $f208: 88
	sta $02                  ; $f209: 85 02 WSYNC
	sta $2a                  ; $f20b: 85 2a HMOVE
	cpx $89                  ; $f20d: e4 89
	bne $f21a                ; $f20f: d0 09
loc_f211:
	lda #$00                 ; $f211: a9 00 VSYNC
	sta $0b                  ; $f213: 85 0b REFP0
	lda $8b                  ; $f215: a5 8b
	jmp $f21c                ; $f217: 4c 1c f2

; === Code Block $f21a-$f24f ===
.org $f21a

loc_f21a:
	lda $8a                  ; $f21a: a5 8a
loc_f21c:
	sta $08                  ; $f21c: 85 08 COLUPF
	lda ($d9),y              ; $f21e: b1 d9
	sta $1c                  ; $f220: 85 1c GRP1
	jsr $f616                ; $f222: 20 16 f6
loc_f225:
	lda ($db),y              ; $f225: b1 db
	sta $1c                  ; $f227: 85 1c GRP1
	dey                      ; $f229: 88
	sta $02                  ; $f22a: 85 02 WSYNC
	sta $2a                  ; $f22c: 85 2a HMOVE
	stx $08                  ; $f22e: 86 08 COLUPF
	lda ($d9),y              ; $f230: b1 d9
	sta $1c                  ; $f232: 85 1c GRP1
	ldx $95                  ; $f234: a6 95
	lda $b6,x                ; $f236: b5 b6
	sta $d9                  ; $f238: 85 d9
	lda $c2,x                ; $f23a: b5 c2
	sta $f6                  ; $f23c: 85 f6
	nop                      ; $f23e: ea
	lda ($db),y              ; $f23f: b1 db
	sta $1c                  ; $f241: 85 1c GRP1
	lda $f6                  ; $f243: a5 f6
	sta $db                  ; $f245: 85 db
	lda #$00                 ; $f247: a9 00 VSYNC
	sta $92                  ; $f249: 85 92
	sta $93                  ; $f24b: 85 93
	sta $0d                  ; $f24d: 85 0d PF0
	jmp $f0f0                ; $f24f: 4c f0 f0

; === Code Block $f252-$f32d ===
.org $f252

loc_f252:
	sta $02                  ; $f252: 85 02 WSYNC
	sta $2a                  ; $f254: 85 2a HMOVE
	lda ($d9),y              ; $f256: b1 d9
	sta $1c                  ; $f258: 85 1c GRP1
	jsr $f615                ; $f25a: 20 15 f6
loc_f25d:
	jsr $f616                ; $f25d: 20 16 f6
loc_f260:
	lda ($db),y              ; $f260: b1 db
	sta $1c                  ; $f262: 85 1c GRP1
	dey                      ; $f264: 88
	bpl $f252                ; $f265: 10 eb
loc_f267:
	ldy #$0f                 ; $f267: a0 0f PF2
	lda $8d                  ; $f269: a5 8d
	sta $02                  ; $f26b: 85 02 WSYNC
	sta $2a                  ; $f26d: 85 2a HMOVE
	cpy #$0f                 ; $f26f: c0 0f PF2
	bne $f275                ; $f271: d0 02
loc_f273:
	lda $8c                  ; $f273: a5 8c
loc_f275:
	sta $09                  ; $f275: 85 09 COLUBK
	lda $b5                  ; $f277: a5 b5
	sta $d9                  ; $f279: 85 d9
	lda $c1                  ; $f27b: a5 c1
	sta $db                  ; $f27d: 85 db
	lda ($d9),y              ; $f27f: b1 d9
	sta $1c                  ; $f281: 85 1c GRP1
	lda ($db),y              ; $f283: b1 db
	dey                      ; $f285: 88
	sta $1c                  ; $f286: 85 1c GRP1
	cpy #$06                 ; $f288: c0 06 COLUP0
	bcs $f269                ; $f28a: b0 dd
loc_f28c:
	sta $02                  ; $f28c: 85 02 WSYNC
	sta $2a                  ; $f28e: 85 2a HMOVE
	lda $8c                  ; $f290: a5 8c
	sta $09                  ; $f292: 85 09 COLUBK
	ldx #$00                 ; $f294: a2 00 VSYNC
	stx $1c                  ; $f296: 86 1c GRP1
	stx $2b                  ; $f298: 86 2b HMCLR
	inx                      ; $f29a: e8
	stx $04                  ; $f29b: 86 04 NUSIZ0
	stx $05                  ; $f29d: 86 05 NUSIZ1
	sta $10                  ; $f29f: 85 10 RESP0
	sta $11                  ; $f2a1: 85 11 RESP1
	lda #$10                 ; $f2a3: a9 10 RESP0
	sta $21                  ; $f2a5: 85 21 HMP1
	lda $88                  ; $f2a7: a5 88
	sta $06                  ; $f2a9: 85 06 COLUP0
	sta $07                  ; $f2ab: 85 07 COLUP1
	ldx #$07                 ; $f2ad: a2 07 COLUP1
	sta $02                  ; $f2af: 85 02 WSYNC
	sta $2a                  ; $f2b1: 85 2a HMOVE
	lda $f6a8,x              ; $f2b3: bd a8 f6
	sta $1b                  ; $f2b6: 85 1b GRP0
	lda $f6b0,x              ; $f2b8: bd b0 f6
	sta $1c                  ; $f2bb: 85 1c GRP1
	nop                      ; $f2bd: ea
	lda $f6c0,x              ; $f2be: bd c0 f6
	tay                      ; $f2c1: a8
	lda $f6b8,x              ; $f2c2: bd b8 f6
	sta $1b                  ; $f2c5: 85 1b GRP0
	sty $1c                  ; $f2c7: 84 1c GRP1
	sta $2b                  ; $f2c9: 85 2b HMCLR
	dex                      ; $f2cb: ca
	bpl $f2af                ; $f2cc: 10 e1
loc_f2ce:
	lda #$1a                 ; $f2ce: a9 1a AUDV1
	sta $0296                ; $f2d0: 8d 96 02 TIM64T
	lda $81                  ; $f2d3: a5 81
	and #$01                 ; $f2d5: 29 01 VBLANK
	tax                      ; $f2d7: aa
	asl                      ; $f2d8: 0a
	tay                      ; $f2d9: a8
	lda $e7,x                ; $f2da: b5 e7
	and #$f0                 ; $f2dc: 29 f0
	lsr                      ; $f2de: 4a
	bne $f2e3                ; $f2df: d0 02
loc_f2e1:
	lda #$50                 ; $f2e1: a9 50 RESP0
loc_f2e3:
	sta $00dd,y              ; $f2e3: 99 dd 00
	lda $e7,x                ; $f2e6: b5 e7
	and #$0f                 ; $f2e8: 29 0f PF2
	asl                      ; $f2ea: 0a
	asl                      ; $f2eb: 0a
	asl                      ; $f2ec: 0a
	sta $00e1,y              ; $f2ed: 99 e1 00
	ldy #$00                 ; $f2f0: a0 00 VSYNC
	jsr $f69e                ; $f2f2: 20 9e f6
loc_f2f5:
	bpl $f317                ; $f2f5: 10 20
loc_f2f7:
	lda $ea,x                ; $f2f7: b5 ea
	beq $f34c                ; $f2f9: f0 51
loc_f2fb:
	and #$40                 ; $f2fb: 29 40 VSYNC
	beq $f330                ; $f2fd: f0 31
loc_f2ff:
	lda #$04                 ; $f2ff: a9 04 NUSIZ0
	sta $15,x                ; $f301: 95 15 AUDC0
	dec $ea,x                ; $f303: d6 ea
	lda $ea,x                ; $f305: b5 ea
	and #$1f                 ; $f307: 29 1f ENABL
	cmp #$10                 ; $f309: c9 10 RESP0
	bcc $f317                ; $f30b: 90 0a
loc_f30d:
	pha                      ; $f30d: 48
	and #$03                 ; $f30e: 29 03 RSYNC
	adc #$02                 ; $f310: 69 02 WSYNC
	sta $17,x                ; $f312: 95 17 AUDF0
	pla                      ; $f314: 68
	ldy #$04                 ; $f315: a0 04 NUSIZ0
loc_f317:
	sty $19,x                ; $f317: 94 19 AUDV0
	cmp #$00                 ; $f319: c9 00 VSYNC
	bne $f321                ; $f31b: d0 04
loc_f31d:
	lda #$00                 ; $f31d: a9 00 VSYNC
	sta $ea,x                ; $f31f: 95 ea
loc_f321:
	lda $0282                ; $f321: ad 82 02 SWCHB
	and $f7fe,x              ; $f324: 3d fe f7
	beq $f32d                ; $f327: f0 04
loc_f329:
	lda #$06                 ; $f329: a9 06 COLUP0
	sta $8e,x                ; $f32b: 95 8e
loc_f32d:
	jmp $f42f                ; $f32d: 4c 2f f4

; === Code Block $f330-$f349 ===
.org $f330

loc_f330:
	lda $ea,x                ; $f330: b5 ea
	sta $19,x                ; $f332: 95 19 AUDV0
	lda #$0c                 ; $f334: a9 0c REFP1
	sta $15,x                ; $f336: 95 15 AUDC0
	txa                      ; $f338: 8a
	adc #$06                 ; $f339: 69 06 COLUP0
	sta $17,x                ; $f33b: 95 17 AUDF0
	dec $ea,x                ; $f33d: d6 ea
	lda $ea,x                ; $f33f: b5 ea
	and #$0f                 ; $f341: 29 0f PF2
	bne $f349                ; $f343: d0 04
loc_f345:
	lda #$00                 ; $f345: a9 00 VSYNC
	sta $ea,x                ; $f347: 95 ea
loc_f349:
	jmp $f42f                ; $f349: 4c 2f f4

; === Code Block $f34c-$f419 ===
.org $f34c

loc_f34c:
	lda $83                  ; $f34c: a5 83
	cmp #$08                 ; $f34e: c9 08 COLUPF
	lda #$02                 ; $f350: a9 02 WSYNC
	bcs $f376                ; $f352: b0 22
loc_f354:
	lda $e6                  ; $f354: a5 e6
	beq $f35e                ; $f356: f0 06
loc_f358:
	lda #$00                 ; $f358: a9 00 VSYNC
	sta $19,x                ; $f35a: 95 19 AUDV0
	beq $f349                ; $f35c: f0 eb
loc_f35e:
	lda $ea                  ; $f35e: a5 ea
	ora $eb                  ; $f360: 05 eb
	bne $f38d                ; $f362: d0 29
loc_f364:
	lda $82                  ; $f364: a5 82
	eor #$40                 ; $f366: 49 40 VSYNC
	cmp #$e0                 ; $f368: c9 e0
	bcc $f38d                ; $f36a: 90 21
loc_f36c:
	lda $82                  ; $f36c: a5 82
	eor $81                  ; $f36e: 45 81
	and #$3f                 ; $f370: 29 3f
	beq $f38d                ; $f372: f0 19
loc_f374:
	lda $82                  ; $f374: a5 82
loc_f376:
	and #$03                 ; $f376: 29 03 RSYNC
	ora #$04                 ; $f378: 09 04 NUSIZ0
	sta $17                  ; $f37a: 85 17 AUDF0
	sec                      ; $f37c: 38
	sbc #$01                 ; $f37d: e9 01 VBLANK
	sta $18                  ; $f37f: 85 18 AUDF1
	lda #$01                 ; $f381: a9 01 VBLANK
	sta $15                  ; $f383: 85 15 AUDC0
	sta $16                  ; $f385: 85 16 AUDC1
	sta $19                  ; $f387: 85 19 AUDV0
	sta $1a                  ; $f389: 85 1a AUDV1
	bne $f349                ; $f38b: d0 bc
loc_f38d:
	lda $8e,x                ; $f38d: b5 8e
	lsr                      ; $f38f: 4a
	lsr                      ; $f390: 4a
	lsr                      ; $f391: 4a
	lsr                      ; $f392: 4a
	tay                      ; $f393: a8
	cpy #$0a                 ; $f394: c0 0a CTRLPF
	bcc $f39a                ; $f396: 90 02
loc_f398:
	ldy #$09                 ; $f398: a0 09 COLUBK
loc_f39a:
	lda #$00                 ; $f39a: a9 00 VSYNC
	cpy #$05                 ; $f39c: c0 05 NUSIZ1
	bcc $f3a2                ; $f39e: 90 02
loc_f3a0:
	lda #$01                 ; $f3a0: a9 01 VBLANK
loc_f3a2:
	sta $fb                  ; $f3a2: 85 fb
	lda $0097,y              ; $f3a4: b9 97 00
	sta $fa                  ; $f3a7: 85 fa
	lsr                      ; $f3a9: 4a
	lsr                      ; $f3aa: 4a
	lsr                      ; $f3ab: 4a
	lsr                      ; $f3ac: 4a
	and #$07                 ; $f3ad: 29 07 COLUP1
	sta $f8                  ; $f3af: 85 f8
	cmp #$02                 ; $f3b1: c9 02 WSYNC
	lda #$20                 ; $f3b3: a9 20 HMP0
	bcc $f3bd                ; $f3b5: 90 06
loc_f3b7:
	lda #$ff                 ; $f3b7: a9 ff
	sta $fb                  ; $f3b9: 85 fb
	lda #$10                 ; $f3bb: a9 10 RESP0
loc_f3bd:
	sta $f7                  ; $f3bd: 85 f7
	lda #$03                 ; $f3bf: a9 03 RSYNC
	sta $15,x                ; $f3c1: 95 15 AUDC0
	lda $00ec,y              ; $f3c3: b9 ec 00
	sta $f9                  ; $f3c6: 85 f9
	lda #$7f                 ; $f3c8: a9 7f
	sta $fd                  ; $f3ca: 85 fd
	lda $fb                  ; $f3cc: a5 fb
	sta $fe                  ; $f3ce: 85 fe
	lda $fa                  ; $f3d0: a5 fa
	and #$07                 ; $f3d2: 29 07 COLUP1
	asl                      ; $f3d4: 0a
	asl                      ; $f3d5: 0a
	ora #$03                 ; $f3d6: 09 03 RSYNC
	tay                      ; $f3d8: a8
	lda $fb                  ; $f3d9: a5 fb
	sta $f6                  ; $f3db: 85 f6
	clc                      ; $f3dd: 18
	lda $f7da,y              ; $f3de: b9 da f7
	adc $f9                  ; $f3e1: 65 f9
	cmp #$a0                 ; $f3e3: c9 a0
	bcc $f3e9                ; $f3e5: 90 02
loc_f3e7:
	sbc #$a0                 ; $f3e7: e9 a0
loc_f3e9:
	sta $fc                  ; $f3e9: 85 fc
	lda $f7f6,x              ; $f3eb: bd f6 f7
	sec                      ; $f3ee: 38
	sbc $fc                  ; $f3ef: e5 fc
	bcs $f3f7                ; $f3f1: b0 04
loc_f3f3:
	eor #$ff                 ; $f3f3: 49 ff
	inc $f6                  ; $f3f5: e6 f6
loc_f3f7:
	cmp $fd                  ; $f3f7: c5 fd
	bcs $f401                ; $f3f9: b0 06
loc_f3fb:
	sta $fd                  ; $f3fb: 85 fd
	lda $f6                  ; $f3fd: a5 f6
	sta $fe                  ; $f3ff: 85 fe
loc_f401:
	dey                      ; $f401: 88
	tya                      ; $f402: 98
	and #$03                 ; $f403: 29 03 RSYNC
	bne $f3d9                ; $f405: d0 d2
loc_f407:
	lda $fd                  ; $f407: a5 fd
	cmp $f7                  ; $f409: c5 f7
	bcc $f41c                ; $f40b: 90 0f
loc_f40d:
	lda #$0f                 ; $f40d: a9 0f PF2
	sta $15,x                ; $f40f: 95 15 AUDC0
	lda #$1f                 ; $f411: a9 1f ENABL
	sta $17,x                ; $f413: 95 17 AUDF0
	lda #$01                 ; $f415: a9 01 VBLANK
	sta $19,x                ; $f417: 95 19 AUDV0
	jmp $f42f                ; $f419: 4c 2f f4

; === Code Block $f41c-$f42f ===
.org $f41c

loc_f41c:
	dec $f7                  ; $f41c: c6 f7
	eor $f7                  ; $f41e: 45 f7
	lsr                      ; $f420: 4a
	lsr                      ; $f421: 4a
	sta $19,x                ; $f422: 95 19 AUDV0
	ldy $fe                  ; $f424: a4 fe
	iny                      ; $f426: c8
	lda $f7f8,y              ; $f427: b9 f8 f7
	clc                      ; $f42a: 18
	adc $f8                  ; $f42b: 65 f8
	sta $17,x                ; $f42d: 95 17 AUDF0

; === Code Block $f42f-$f4cb ===
.org $f42f

loc_f42f:
	lda $81                  ; $f42f: a5 81
	and #$1f                 ; $f431: 29 1f ENABL
	bne $f43f                ; $f433: d0 0a
loc_f435:
	lda $82                  ; $f435: a5 82
	asl                      ; $f437: 0a
	asl                      ; $f438: 0a
	asl                      ; $f439: 0a
	eor $82                  ; $f43a: 45 82
	asl                      ; $f43c: 0a
	rol $82                  ; $f43d: 26 82
loc_f43f:
	lda $e6                  ; $f43f: a5 e6
	bne $f451                ; $f441: d0 0e
loc_f443:
	ldx #$09                 ; $f443: a2 09 COLUBK
	lda #$ff                 ; $f445: a9 ff
	sta $96                  ; $f447: 85 96
	jsr $f624                ; $f449: 20 24 f6
loc_f44c:
	dex                      ; $f44c: ca
	cpx #$05                 ; $f44d: e0 05 NUSIZ1
	bcs $f449                ; $f44f: b0 f8
loc_f451:
	lda $0284                ; $f451: ad 84 02 INTIM
	bne $f451                ; $f454: d0 fb
loc_f456:
	ldy #$82                 ; $f456: a0 82
	sty $02                  ; $f458: 84 02 WSYNC
	sty $01                  ; $f45a: 84 01 VBLANK
	sty $00                  ; $f45c: 84 00 VSYNC
	sty $02                  ; $f45e: 84 02 WSYNC
	sty $02                  ; $f460: 84 02 WSYNC
	sty $02                  ; $f462: 84 02 WSYNC
	sta $00                  ; $f464: 85 00 VSYNC
	inc $81                  ; $f466: e6 81
	bne $f473                ; $f468: d0 09
loc_f46a:
	inc $e9                  ; $f46a: e6 e9
	inc $e5                  ; $f46c: e6 e5
	bne $f473                ; $f46e: d0 03
loc_f470:
	sec                      ; $f470: 38
	ror $e5                  ; $f471: 66 e5
loc_f473:
	ldy #$ff                 ; $f473: a0 ff
	lda $0282                ; $f475: ad 82 02 SWCHB
	and #$08                 ; $f478: 29 08 COLUPF
	bne $f47e                ; $f47a: d0 02
loc_f47c:
	ldy #$0f                 ; $f47c: a0 0f PF2
loc_f47e:
	tya                      ; $f47e: 98
	ldy #$00                 ; $f47f: a0 00 VSYNC
	bit $e5                  ; $f481: 24 e5
	bpl $f489                ; $f483: 10 04
loc_f485:
	and #$f7                 ; $f485: 29 f7
	ldy $e5                  ; $f487: a4 e5
loc_f489:
	sty $86                  ; $f489: 84 86
	asl $86                  ; $f48b: 06 86
	sta $87                  ; $f48d: 85 87
	lda #$2c                 ; $f48f: a9 2c CXCLR
	sta $02                  ; $f491: 85 02 WSYNC
	sta $0296                ; $f493: 8d 96 02 TIM64T
	lda $e6                  ; $f496: a5 e6
	bne $f4a6                ; $f498: d0 0c
loc_f49a:
	ldx #$04                 ; $f49a: a2 04 NUSIZ0
	lda #$01                 ; $f49c: a9 01 VBLANK
	sta $96                  ; $f49e: 85 96
	jsr $f624                ; $f4a0: 20 24 f6
loc_f4a3:
	dex                      ; $f4a3: ca
	bpl $f4a0                ; $f4a4: 10 fa
loc_f4a6:
	lda $0280                ; $f4a6: ad 80 02 SWCHA
	tay                      ; $f4a9: a8
	and #$0f                 ; $f4aa: 29 0f PF2
	sta $85                  ; $f4ac: 85 85
	tya                      ; $f4ae: 98
	lsr                      ; $f4af: 4a
	lsr                      ; $f4b0: 4a
	lsr                      ; $f4b1: 4a
	lsr                      ; $f4b2: 4a
	sta $84                  ; $f4b3: 85 84
	iny                      ; $f4b5: c8
	beq $f4bc                ; $f4b6: f0 04
loc_f4b8:
	lda #$00                 ; $f4b8: a9 00 VSYNC
	sta $e5                  ; $f4ba: 85 e5
loc_f4bc:
	lda $82                  ; $f4bc: a5 82
	bne $f4c4                ; $f4be: d0 04
loc_f4c0:
	inc $82                  ; $f4c0: e6 82
	bne $f4dc                ; $f4c2: d0 18
loc_f4c4:
	jsr $f69e                ; $f4c4: 20 9e f6
loc_f4c7:
	bmi $f4ce                ; $f4c7: 30 05
loc_f4c9:
	ldx #$e5                 ; $f4c9: a2 e5
	jmp $f004                ; $f4cb: 4c 04 f0

; === Code Block $f4ce-$f4dc ===
.org $f4ce

loc_f4ce:
	ldy #$00                 ; $f4ce: a0 00 VSYNC
	bcs $f4f5                ; $f4d0: b0 23
loc_f4d2:
	lda $83                  ; $f4d2: a5 83
	beq $f4da                ; $f4d4: f0 04
loc_f4d6:
	dec $83                  ; $f4d6: c6 83
	bpl $f4f7                ; $f4d8: 10 1d
loc_f4da:
	inc $80                  ; $f4da: e6 80

; === Code Block $f4dc-$f4fb ===
.org $f4dc

loc_f4dc:
	jsr $f5b0                ; $f4dc: 20 b0 f5
loc_f4df:
	lda $80                  ; $f4df: a5 80
	and #$07                 ; $f4e1: 29 07 COLUP1
	sta $80                  ; $f4e3: 85 80
	sta $e5                  ; $f4e5: 85 e5
	ora #$a0                 ; $f4e7: 09 a0
	tay                      ; $f4e9: a8
	iny                      ; $f4ea: c8
	sty $e7                  ; $f4eb: 84 e7
	lda #$aa                 ; $f4ed: a9 aa
	sta $e8                  ; $f4ef: 85 e8
	ldy #$1e                 ; $f4f1: a0 1e ENAM1
	sty $e6                  ; $f4f3: 84 e6
	sty $83                  ; $f4f5: 84 83
	lda $e6                  ; $f4f7: a5 e6
	beq $f4fe                ; $f4f9: f0 03
loc_f4fb:
	jmp $f00f                ; $f4fb: 4c 0f f0

; === Code Block $f4fe-$f5ad ===
.org $f4fe

loc_f4fe:
	ldx #$01                 ; $f4fe: a2 01 VBLANK
	lda $ea,x                ; $f500: b5 ea
	beq $f50a                ; $f502: f0 06
loc_f504:
	and #$10                 ; $f504: 29 10 RESP0
	bne $f528                ; $f506: d0 20
loc_f508:
	beq $f534                ; $f508: f0 2a
loc_f50a:
	lda $84,x                ; $f50a: b5 84
	lsr                      ; $f50c: 4a
	bcs $f525                ; $f50d: b0 16
loc_f50f:
	inc $8e,x                ; $f50f: f6 8e
	ldy $8e,x                ; $f511: b4 8e
	cpy #$b2                 ; $f513: c0 b2
	bcc $f525                ; $f515: 90 0e
loc_f517:
	sed                      ; $f517: f8
	lda $e7,x                ; $f518: b5 e7
	adc #$00                 ; $f51a: 69 00 VSYNC
	sta $e7,x                ; $f51c: 95 e7
	cld                      ; $f51e: d8
	lda #$8f                 ; $f51f: a9 8f
	sta $ea,x                ; $f521: 95 ea
	bne $f530                ; $f523: d0 0b
loc_f525:
	lsr                      ; $f525: 4a
	bcs $f534                ; $f526: b0 0c
loc_f528:
	dec $8e,x                ; $f528: d6 8e
	lda $8e,x                ; $f52a: b5 8e
	cmp #$06                 ; $f52c: c9 06 COLUP0
	bcs $f534                ; $f52e: b0 04
loc_f530:
	lda #$06                 ; $f530: a9 06 COLUP0
	sta $8e,x                ; $f532: 95 8e
loc_f534:
	lda $ea,x                ; $f534: b5 ea
	and #$1f                 ; $f536: 29 1f ENABL
	cmp #$17                 ; $f538: c9 17 AUDF0
	bcs $f544                ; $f53a: b0 08
loc_f53c:
	lda $90,x                ; $f53c: b5 90
	bmi $f544                ; $f53e: 30 04
loc_f540:
	lda #$5c                 ; $f540: a9 5c GRP1
	sta $ea,x                ; $f542: 95 ea
loc_f544:
	dex                      ; $f544: ca
	bpl $f500                ; $f545: 10 b9
loc_f547:
	ldx #$00                 ; $f547: a2 00 VSYNC
	jsr $f671                ; $f549: 20 71 f6
loc_f54c:
	sta $00b5,y              ; $f54c: 99 b5 00
	cpy #$0b                 ; $f54f: c0 0b REFP0
	beq $f559                ; $f551: f0 06
loc_f553:
	clc                      ; $f553: 18
	adc #$10                 ; $f554: 69 10 RESP0
	sta $00b6,y              ; $f556: 99 b6 00
loc_f559:
	inx                      ; $f559: e8
	jsr $f671                ; $f55a: 20 71 f6
loc_f55d:
	sta $00c1,y              ; $f55d: 99 c1 00
	cpy #$0b                 ; $f560: c0 0b REFP0
	beq $f56a                ; $f562: f0 06
loc_f564:
	clc                      ; $f564: 18
	adc #$10                 ; $f565: 69 10 RESP0
	sta $00c2,y              ; $f567: 99 c2 00
loc_f56a:
	lda $81                  ; $f56a: a5 81
	and #$70                 ; $f56c: 29 70
	bne $f5ad                ; $f56e: d0 3d
loc_f570:
	lda $80                  ; $f570: a5 80
	and #$04                 ; $f572: 29 04 NUSIZ0
	beq $f5ad                ; $f574: f0 37
loc_f576:
	lda $81                  ; $f576: a5 81
	and #$0f                 ; $f578: 29 0f PF2
	tax                      ; $f57a: aa
	cpx #$0a                 ; $f57b: e0 0a CTRLPF
	bcs $f5ad                ; $f57d: b0 2e
loc_f57f:
	lda $97,x                ; $f57f: b5 97
	lsr                      ; $f581: 4a
	lsr                      ; $f582: 4a
	lsr                      ; $f583: 4a
	lsr                      ; $f584: 4a
	and #$07                 ; $f585: 29 07 COLUP1
	tay                      ; $f587: a8
	lda $81                  ; $f588: a5 81
	eor $82                  ; $f58a: 45 82
	lsr                      ; $f58c: 4a
	bcc $f594                ; $f58d: 90 05
loc_f58f:
	dey                      ; $f58f: 88
	bpl $f594                ; $f590: 10 02
loc_f592:
	ldy #$00                 ; $f592: a0 00 VSYNC
loc_f594:
	lsr                      ; $f594: 4a
	bcc $f59e                ; $f595: 90 07
loc_f597:
	iny                      ; $f597: c8
	cpy #$06                 ; $f598: c0 06 COLUP0
	bcc $f59e                ; $f59a: 90 02
loc_f59c:
	ldy #$05                 ; $f59c: a0 05 NUSIZ1
loc_f59e:
	tya                      ; $f59e: 98
	asl                      ; $f59f: 0a
	asl                      ; $f5a0: 0a
	asl                      ; $f5a1: 0a
	asl                      ; $f5a2: 0a
	sta $f6                  ; $f5a3: 85 f6
	lda $97,x                ; $f5a5: b5 97
	and #$8f                 ; $f5a7: 29 8f
	ora $f6                  ; $f5a9: 05 f6
	sta $97,x                ; $f5ab: 95 97
loc_f5ad:
	jmp $f00f                ; $f5ad: 4c 0f f0

; === Code Block $f5b0-$f5f7 ===
.org $f5b0

loc_f5b0:
	lda $81                  ; $f5b0: a5 81
	and #$01                 ; $f5b2: 29 01 VBLANK
	sta $81                  ; $f5b4: 85 81
	ldx #$01                 ; $f5b6: a2 01 VBLANK
	lda #$06                 ; $f5b8: a9 06 COLUP0
	sta $8e,x                ; $f5ba: 95 8e
	lda #$00                 ; $f5bc: a9 00 VSYNC
	sta $19,x                ; $f5be: 95 19 AUDV0
	dex                      ; $f5c0: ca
	bpl $f5b8                ; $f5c1: 10 f5
loc_f5c3:
	ldx #$0d                 ; $f5c3: a2 0d PF0
	lda #$f7                 ; $f5c5: a9 f7
	sta $d7,x                ; $f5c7: 95 d7
	dex                      ; $f5c9: ca
	dex                      ; $f5ca: ca
	bpl $f5c7                ; $f5cb: 10 fa
loc_f5cd:
	ldx #$09                 ; $f5cd: a2 09 COLUBK
	lda #$01                 ; $f5cf: a9 01 VBLANK
	sta $a1,x                ; $f5d1: 95 a1
	lda $f6f6,x              ; $f5d3: bd f6 f6
	sta $cd,x                ; $f5d6: 95 cd
	clc                      ; $f5d8: 18
	lda $80                  ; $f5d9: a5 80
	and #$03                 ; $f5db: 29 03 RSYNC
	tay                      ; $f5dd: a8
	txa                      ; $f5de: 8a
	adc $f7d6,y              ; $f5df: 79 d6 f7
	tay                      ; $f5e2: a8
	lda $f6c8,y              ; $f5e3: b9 c8 f6
	sta $97,x                ; $f5e6: 95 97
	lda #$60                 ; $f5e8: a9 60 HMP0
	sta $ab,x                ; $f5ea: 95 ab
	lda #$50                 ; $f5ec: a9 50 RESP0
	sta $b5,x                ; $f5ee: 95 b5
	sta $b9,x                ; $f5f0: 95 b9
	sta $c3,x                ; $f5f2: 95 c3
	dex                      ; $f5f4: ca
	bpl $f5cf                ; $f5f5: 10 d8
loc_f5f7:
	rts                      ; $f5f7: 60

; === Code Block $f5f8-$f613 ===
.org $f5f8

loc_f5f8:
	clc                      ; $f5f8: 18
	adc #$2e                 ; $f5f9: 69 2e
	tay                      ; $f5fb: a8
	and #$0f                 ; $f5fc: 29 0f PF2
	sta $f6                  ; $f5fe: 85 f6
	tya                      ; $f600: 98
	lsr                      ; $f601: 4a
	lsr                      ; $f602: 4a
	lsr                      ; $f603: 4a
	lsr                      ; $f604: 4a
	tay                      ; $f605: a8
	clc                      ; $f606: 18
	adc $f6                  ; $f607: 65 f6
	cmp #$0f                 ; $f609: c9 0f PF2
	bcc $f610                ; $f60b: 90 03
loc_f60d:
	sbc #$0f                 ; $f60d: e9 0f PF2
	iny                      ; $f60f: c8
loc_f610:
	eor #$07                 ; $f610: 49 07 COLUP1
	asl                      ; $f612: 0a

; === Code Block $f613-$f616 ===
.org $f613

loc_f613:
	asl                      ; $f613: 0a
	asl                      ; $f614: 0a
loc_f615:
	asl                      ; $f615: 0a
loc_f616:
	rts                      ; $f616: 60

; === Code Block $f617-$f623 ===
.org $f617

loc_f617:
	jsr $f5f8                ; $f617: 20 f8 f5
loc_f61a:
	sta $20,x                ; $f61a: 95 20 HMP0
	sta $02                  ; $f61c: 85 02 WSYNC
	dey                      ; $f61e: 88
	bpl $f61e                ; $f61f: 10 fd
loc_f621:
	sta $10,x                ; $f621: 95 10 RESP0
	rts                      ; $f623: 60

; === Code Block $f624-$f670 ===
.org $f624

loc_f624:
	dec $a1,x                ; $f624: d6 a1
	bpl $f653                ; $f626: 10 2b
loc_f628:
	lda $97,x                ; $f628: b5 97
	lsr                      ; $f62a: 4a
	lsr                      ; $f62b: 4a
	lsr                      ; $f62c: 4a
	lsr                      ; $f62d: 4a
	and #$07                 ; $f62e: 29 07 COLUP1
	sec                      ; $f630: 38
	sbc #$01                 ; $f631: e9 01 VBLANK
	bpl $f63e                ; $f633: 10 09
loc_f635:
	lda $ec,x                ; $f635: b5 ec
	clc                      ; $f637: 18
	adc $96                  ; $f638: 65 96
	sta $ec,x                ; $f63a: 95 ec
	lda #$00                 ; $f63c: a9 00 VSYNC
loc_f63e:
	sta $a1,x                ; $f63e: 95 a1
	lda $ec,x                ; $f640: b5 ec
	clc                      ; $f642: 18
	adc $96                  ; $f643: 65 96
	cmp #$c8                 ; $f645: c9 c8
	bcc $f64b                ; $f647: 90 02
loc_f649:
	lda #$9f                 ; $f649: a9 9f
loc_f64b:
	cmp #$a0                 ; $f64b: c9 a0
	bcc $f651                ; $f64d: 90 02
loc_f64f:
	lda #$00                 ; $f64f: a9 00 VSYNC
loc_f651:
	sta $ec,x                ; $f651: 95 ec
loc_f653:
	lda $ec,x                ; $f653: b5 ec
	jsr $f5f8                ; $f655: 20 f8 f5
loc_f658:
	sta $f6                  ; $f658: 85 f6
	dey                      ; $f65a: 88
	dey                      ; $f65b: 88
	dey                      ; $f65c: 88
	asl $97,x                ; $f65d: 16 97
	cpy #$06                 ; $f65f: c0 06 COLUP0
	ror $97,x                ; $f661: 76 97
	tya                      ; $f663: 98
	ora $f6                  ; $f664: 05 f6
	sta $ab,x                ; $f666: 95 ab
	lda #$50                 ; $f668: a9 50 RESP0
	sta $b5,x                ; $f66a: 95 b5
	sta $c3,x                ; $f66c: 95 c3
	sta $b9,x                ; $f66e: 95 b9
	rts                      ; $f670: 60

; === Code Block $f671-$f69d ===
.org $f671

loc_f671:
	lda $8e,x                ; $f671: b5 8e
	lsr                      ; $f673: 4a
	lsr                      ; $f674: 4a
	lsr                      ; $f675: 4a
	lsr                      ; $f676: 4a
	tay                      ; $f677: a8
	lda $8e,x                ; $f678: b5 8e
	and #$0f                 ; $f67a: 29 0f PF2
	sta $f6                  ; $f67c: 85 f6
	lda $ea,x                ; $f67e: b5 ea
	beq $f68f                ; $f680: f0 0d
loc_f682:
	and #$40                 ; $f682: 29 40 VSYNC
	beq $f68f                ; $f684: f0 09
loc_f686:
	lda $8e,x                ; $f686: b5 8e
	lsr                      ; $f688: 4a
	lsr                      ; $f689: 4a
	lsr                      ; $f68a: 4a
	lda #$a0                 ; $f68b: a9 a0
	bcc $f69a                ; $f68d: 90 0b
loc_f68f:
	lda $f6                  ; $f68f: a5 f6
	lsr                      ; $f691: 4a
	lsr                      ; $f692: 4a
	lsr                      ; $f693: 4a
	lda #$60                 ; $f694: a9 60 HMP0
	bcc $f69a                ; $f696: 90 02
loc_f698:
	lda #$80                 ; $f698: a9 80
loc_f69a:
	sec                      ; $f69a: 38
	sbc $f6                  ; $f69b: e5 f6
	rts                      ; $f69d: 60

; === Code Block $f69e-$f6a3 ===
.org $f69e

loc_f69e:
	lda $0282                ; $f69e: ad 82 02 SWCHB
	lsr                      ; $f6a1: 4a
	ror                      ; $f6a2: 6a
	rts                      ; $f6a3: 60

; === Vectors ===
.org $fffc
	.word reset
	.word reset
