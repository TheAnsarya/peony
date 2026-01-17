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
AUDC1            = $0016
AUDV1            = $001a
GRP0             = $001b
GRP1             = $001c
ENAM1            = $001e
ENABL            = $001f
HMP0             = $0020
VDELP1           = $0026
HMOVE            = $002a
HMCLR            = $002b
CXCLR            = $002c
WSYNC            = $0042
NUSIZ1           = $0045
AUDF1            = $0058
SWCHB            = $0282
INTIM            = $0284

; === Code Block $f000-$f139 ===
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
	jsr $ff0c                ; $f00c: 20 0c ff
loc_f00f:
	lda #$fb                 ; $f00f: a9 fb
	ldx #$0b                 ; $f011: a2 0b REFP0
	jsr $ff1c                ; $f013: 20 1c ff
loc_f016:
	ldx #$26                 ; $f016: a2 26 VDELP1
	jsr $fa8f                ; $f018: 20 8f fa
loc_f01b:
	lda $83                  ; $f01b: a5 83
	bne $f027                ; $f01d: d0 08
loc_f01f:
	inc $83                  ; $f01f: e6 83
	sta $c0                  ; $f021: 85 c0
	lda #$08                 ; $f023: a9 08 COLUPF
	sta $d7                  ; $f025: 85 d7
loc_f027:
	ldx #$04                 ; $f027: a2 04 NUSIZ0
	lda $b7                  ; $f029: a5 b7
	lsr                      ; $f02b: 4a
	lsr                      ; $f02c: 4a
	lsr                      ; $f02d: 4a
	clc                      ; $f02e: 18
	adc #$45                 ; $f02f: 69 45 NUSIZ1
	jsr $faef                ; $f031: 20 ef fa
loc_f034:
	inx                      ; $f034: e8
	lda $fff7,x              ; $f035: bd f7 ff
	eor $85                  ; $f038: 45 85
	and $86                  ; $f03a: 25 86
	sta $ee,x                ; $f03c: 95 ee
	sta $05,x                ; $f03e: 95 05 NUSIZ1
	dex                      ; $f040: ca
	bpl $f035                ; $f041: 10 f2
loc_f043:
	tay                      ; $f043: a8
	lda $d7                  ; $f044: a5 d7
	cmp #$10                 ; $f046: c9 10 RESP0
	beq $f058                ; $f048: f0 0e
loc_f04a:
	lda $0282                ; $f04a: ad 82 02 SWCHB
	lsr                      ; $f04d: 4a
	bcc $f058                ; $f04e: 90 08
loc_f050:
	lda $f4                  ; $f050: a5 f4
	beq $f058                ; $f052: f0 04
loc_f054:
	sty $ef                  ; $f054: 84 ef
	sty $06                  ; $f056: 84 06 COLUP0
loc_f058:
	lda $8d                  ; $f058: a5 8d
	beq $f067                ; $f05a: f0 0b
loc_f05c:
	dec $8d                  ; $f05c: c6 8d
	lsr                      ; $f05e: 4a
	bcc $f067                ; $f05f: 90 06
loc_f061:
	lda #$42                 ; $f061: a9 42 WSYNC
	and $86                  ; $f063: 25 86
	sta $09                  ; $f065: 85 09 COLUBK
loc_f067:
	inx                      ; $f067: e8
	stx $f2                  ; $f068: 86 f2
	stx $04                  ; $f06a: 86 04 NUSIZ0
	ldy $b3                  ; $f06c: a4 b3
	lda $e0                  ; $f06e: a5 e0
	sta $0b                  ; $f070: 85 0b REFP0
	beq $f075                ; $f072: f0 01
loc_f074:
	iny                      ; $f074: c8
loc_f075:
	tya                      ; $f075: 98
	jsr $faef                ; $f076: 20 ef fa
loc_f079:
	inx                      ; $f079: e8
	stx $0a                  ; $f07a: 86 0a CTRLPF
	stx $26                  ; $f07c: 86 26 VDELP1
	ldy $99                  ; $f07e: a4 99
	lda $9f                  ; $f080: a5 9f
	sta $05                  ; $f082: 85 05 NUSIZ1
	sta $0c                  ; $f084: 85 0c REFP1
	jsr $faf2                ; $f086: 20 f2 fa
loc_f089:
	inx                      ; $f089: e8
	lda $f5                  ; $f08a: a5 f5
	jsr $faef                ; $f08c: 20 ef fa
loc_f08f:
	jsr $ffae                ; $f08f: 20 ae ff
loc_f092:
	sty $0d                  ; $f092: 84 0d PF0
	sty $e2                  ; $f094: 84 e2
	sty $e4                  ; $f096: 84 e4
	sty $e6                  ; $f098: 84 e6
	sty $e8                  ; $f09a: 84 e8
	ldx #$05                 ; $f09c: a2 05 NUSIZ1
	jsr $fbd5                ; $f09e: 20 d5 fb
loc_f0a1:
	lda $8b                  ; $f0a1: a5 8b
	cmp #$03                 ; $f0a3: c9 03 RSYNC
	bcs $f0a8                ; $f0a5: b0 01
loc_f0a7:
	dex                      ; $f0a7: ca
loc_f0a8:
	stx $de                  ; $f0a8: 86 de
	ldy $a0,x                ; $f0aa: b4 a0
	ldx $ffc8,y              ; $f0ac: be c8 ff
	stx $c7                  ; $f0af: 86 c7
	ldx $fbbb,y              ; $f0b1: be bb fb
	stx $c9                  ; $f0b4: 86 c9
	ldx $ff23,y              ; $f0b6: be 23 ff
	stx $cb                  ; $f0b9: 86 cb
	sta $2c                  ; $f0bb: 85 2c CXCLR
	sta $2b                  ; $f0bd: 85 2b HMCLR
	tax                      ; $f0bf: aa
	sec                      ; $f0c0: 38
	sbc #$01                 ; $f0c1: e9 01 VBLANK
	and #$1f                 ; $f0c3: 29 1f ENABL
	sta $fc                  ; $f0c5: 85 fc
	lsr $fc                  ; $f0c7: 46 fc
	cmp #$1a                 ; $f0c9: c9 1a AUDV1
	bcc $f0d1                ; $f0cb: 90 04
loc_f0cd:
	sbc #$16                 ; $f0cd: e9 16 AUDC1
	bne $f0d9                ; $f0cf: d0 08
loc_f0d1:
	cmp #$04                 ; $f0d1: c9 04 NUSIZ0
	bcc $f0d9                ; $f0d3: 90 04
loc_f0d5:
	and #$01                 ; $f0d5: 29 01 VBLANK
	ora #$02                 ; $f0d7: 09 02 WSYNC
loc_f0d9:
	tay                      ; $f0d9: a8
	lda $fbef,y              ; $f0da: b9 ef fb
	pha                      ; $f0dd: 48
	lda $fdf6,y              ; $f0de: b9 f6 fd
	pha                      ; $f0e1: 48
	txa                      ; $f0e2: 8a
	lsr                      ; $f0e3: 4a
	tay                      ; $f0e4: a8
	lda ($c7),y              ; $f0e5: b1 c7
	bcc $f0eb                ; $f0e7: 90 02
loc_f0e9:
	lda ($c9),y              ; $f0e9: b1 c9
loc_f0eb:
	cpx #$1a                 ; $f0eb: e0 1a AUDV1
	bcs $f0f9                ; $f0ed: b0 0a
loc_f0ef:
	cpx #$03                 ; $f0ef: e0 03 RSYNC
	bcc $f0f9                ; $f0f1: 90 06
loc_f0f3:
	sta $1c                  ; $f0f3: 85 1c GRP1
	lda #$00                 ; $f0f5: a9 00 VSYNC
	sta $1b                  ; $f0f7: 85 1b GRP0
loc_f0f9:
	lda ($d9),y              ; $f0f9: b1 d9
	sta $0e                  ; $f0fb: 85 0e PF1
	lda ($db),y              ; $f0fd: b1 db
	sta $0f                  ; $f0ff: 85 0f PF2
	lda ($cb),y              ; $f101: b1 cb
	eor $85                  ; $f103: 45 85
	and $86                  ; $f105: 25 86
	sta $07                  ; $f107: 85 07 COLUP1
	ldx $de                  ; $f109: a6 de
	lda $8e,x                ; $f10b: b5 8e
	sta $ed                  ; $f10d: 85 ed
	and #$04                 ; $f10f: 29 04 NUSIZ0
	ora #$d2                 ; $f111: 09 d2
	eor $85                  ; $f113: 45 85
	and $86                  ; $f115: 25 86
	sta $ee                  ; $f117: 85 ee
	bit $93                  ; $f119: 24 93
	bpl $f128                ; $f11b: 10 0b
loc_f11d:
	cpy #$0d                 ; $f11d: c0 0d PF0
	bcs $f128                ; $f11f: b0 07
loc_f121:
	lda $ffb3,y              ; $f121: b9 b3 ff
	eor $85                  ; $f124: 45 85
	and $86                  ; $f126: 25 86
loc_f128:
	sta $08                  ; $f128: 85 08 COLUPF
	ldy #$a0                 ; $f12a: a0 a0
	sty $fd                  ; $f12c: 84 fd
	lda $0284                ; $f12e: ad 84 02 INTIM
	bne $f12e                ; $f131: d0 fb
loc_f133:
	sta $02                  ; $f133: 85 02 WSYNC
	sta $2a                  ; $f135: 85 2a HMOVE
	sta $01                  ; $f137: 85 01 VBLANK
	rts                      ; $f139: 60

; === Code Block $fa8f-$fab0 ===
.org $fa8f

loc_fa8f:
	lda $fdb1,x              ; $fa8f: bd b1 fd
	sta $a6,x                ; $fa92: 95 a6
	dex                      ; $fa94: ca
	bpl $fa8f                ; $fa95: 10 f8
loc_fa97:
	lda #$00                 ; $fa97: a9 00 VSYNC
	ldx #$1e                 ; $fa99: a2 1e ENAM1
	sta $87,x                ; $fa9b: 95 87
	dex                      ; $fa9d: ca
	bpl $fa9b                ; $fa9e: 10 fb
loc_faa0:
	ldx #$05                 ; $faa0: a2 05 NUSIZ1
	ldy #$01                 ; $faa2: a0 01 VBLANK
	lda $bd                  ; $faa4: a5 bd
	lsr                      ; $faa6: 4a
	bcc $faab                ; $faa7: 90 02
loc_faa9:
	ldy #$05                 ; $faa9: a0 05 NUSIZ1
loc_faab:
	sty $8e,x                ; $faab: 94 8e
	dex                      ; $faad: ca
	bpl $faab                ; $faae: 10 fb
loc_fab0:
	rts                      ; $fab0: 60

; === Code Block $faef-$fafe ===
.org $faef

loc_faef:
	jsr $fdd8                ; $faef: 20 d8 fd
loc_faf2:
	sta $20,x                ; $faf2: 95 20 HMP0
	iny                      ; $faf4: c8
	iny                      ; $faf5: c8
	iny                      ; $faf6: c8
	sta $02                  ; $faf7: 85 02 WSYNC
	dey                      ; $faf9: 88
	bpl $faf9                ; $fafa: 10 fd
loc_fafc:
	sta $10,x                ; $fafc: 95 10 RESP0
	rts                      ; $fafe: 60

; === Code Block $fbd5-$fbee ===
.org $fbd5

loc_fbd5:
	lda $a6,x                ; $fbd5: b5 a6
	sta $d9                  ; $fbd7: 85 d9
	lda $8e,x                ; $fbd9: b5 8e
	and #$01                 ; $fbdb: 29 01 VBLANK
	ora #$fc                 ; $fbdd: 09 fc
	sta $da                  ; $fbdf: 85 da
	lda $ac,x                ; $fbe1: b5 ac
	sta $db                  ; $fbe3: 85 db
	lda $8e,x                ; $fbe5: b5 8e
	lsr                      ; $fbe7: 4a
	and #$01                 ; $fbe8: 29 01 VBLANK
	ora #$fc                 ; $fbea: 09 fc
	sta $dc                  ; $fbec: 85 dc
	rts                      ; $fbee: 60

; === Code Block $fdd8-$fdf5 ===
.org $fdd8

loc_fdd8:
	tay                      ; $fdd8: a8
	iny                      ; $fdd9: c8
	tya                      ; $fdda: 98
	and #$0f                 ; $fddb: 29 0f PF2
	sta $ed                  ; $fddd: 85 ed
	tya                      ; $fddf: 98
	lsr                      ; $fde0: 4a
	lsr                      ; $fde1: 4a
	lsr                      ; $fde2: 4a
	lsr                      ; $fde3: 4a
	tay                      ; $fde4: a8
	clc                      ; $fde5: 18
	adc $ed                  ; $fde6: 65 ed
	cmp #$0f                 ; $fde8: c9 0f PF2
	bcc $fdef                ; $fdea: 90 03
loc_fdec:
	sbc #$0f                 ; $fdec: e9 0f PF2
	iny                      ; $fdee: c8
loc_fdef:
	eor #$07                 ; $fdef: 49 07 COLUP1
	asl                      ; $fdf1: 0a
	asl                      ; $fdf2: 0a
	asl                      ; $fdf3: 0a
	asl                      ; $fdf4: 0a
	rts                      ; $fdf5: 60

; === Code Block $ff0c-$ff22 ===
.org $ff0c

loc_ff0c:
	sta $d7                  ; $ff0c: 85 d7
	sta $e7                  ; $ff0e: 85 e7
	lda #$58                 ; $ff10: a9 58 AUDF1
	ldx #$08                 ; $ff12: a2 08 COLUPF
	sta $dd,x                ; $ff14: 95 dd
	dex                      ; $ff16: ca
	dex                      ; $ff17: ca
	bpl $ff14                ; $ff18: 10 fa
loc_ff1a:
	ldx #$08                 ; $ff1a: a2 08 COLUPF
loc_ff1c:
	sta $cd,x                ; $ff1c: 95 cd
	dex                      ; $ff1e: ca
	dex                      ; $ff1f: ca
	bpl $ff1c                ; $ff20: 10 fa
loc_ff22:
	rts                      ; $ff22: 60

; === Code Block $ffae-$ffb2 ===
.org $ffae

loc_ffae:
	sta $02                  ; $ffae: 85 02 WSYNC
	sta $2a                  ; $ffb0: 85 2a HMOVE
	rts                      ; $ffb2: 60

; === Vectors ===
.org $fffc
	.word reset
	.word reset
