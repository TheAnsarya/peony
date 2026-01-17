; 🌺 Peony Disassembly
; ROM: Adventure - Odyssey 2600 (Adventure Hack).a26
; Platform: Atari 2600
; Size: 4096 bytes

; === Block $f000-$f000 (Code) ===
reset:
	jmp $f2ef                ; $f000: 4c ef f2

; === Block $f008-$f05c (Code) ===
loc_f008:
	sta $2b                  ; $f008: 85 2b HMCLR
	lda $86                  ; $f00a: a5 86
	ldx #$00                 ; $f00c: a2 00 VSYNC
	jsr $f0d2                ; $f00e: 20 d2 f0
loc_f011:
	lda $88                  ; $f011: a5 88
	ldx #$01                 ; $f013: a2 01 VBLANK
	jsr $f0d2                ; $f015: 20 d2 f0
loc_f018:
	lda $8b                  ; $f018: a5 8b
	ldx #$04                 ; $f01a: a2 04 NUSIZ0
	jsr $f0d2                ; $f01c: 20 d2 f0
loc_f01f:
	sta $02                  ; $f01f: 85 02 WSYNC
	sta $2a                  ; $f021: 85 2a HMOVE
	sta $2c                  ; $f023: 85 2c CXCLR
	lda $8c                  ; $f025: a5 8c
	sec                      ; $f027: 38
	sbc #$04                 ; $f028: e9 04 NUSIZ0
	sta $8d                  ; $f02a: 85 8d
	lda $0284                ; $f02c: ad 84 02 INTIM
	bne $f02c                ; $f02f: d0 fb
loc_f031:
	lda #$00                 ; $f031: a9 00 VSYNC
	sta $90                  ; $f033: 85 90
	sta $91                  ; $f035: 85 91
	sta $8f                  ; $f037: 85 8f
	sta $1c                  ; $f039: 85 1c GRP1
	lda #$01                 ; $f03b: a9 01 VBLANK
	sta $26                  ; $f03d: 85 26 VDELP1
	lda #$68                 ; $f03f: a9 68 RESMP0
	sta $8e                  ; $f041: 85 8e
	ldy $8f                  ; $f043: a4 8f
	lda ($80),y              ; $f045: b1 80
	sta $0d                  ; $f047: 85 0d PF0
	iny                      ; $f049: c8
	lda ($80),y              ; $f04a: b1 80
	sta $0e                  ; $f04c: 85 0e PF1
	iny                      ; $f04e: c8
	lda ($80),y              ; $f04f: b1 80
	sta $0f                  ; $f051: 85 0f PF2
	iny                      ; $f053: c8
	sty $8f                  ; $f054: 84 8f
	sta $02                  ; $f056: 85 02 WSYNC
	lda #$00                 ; $f058: a9 00 VSYNC
	sta $01                  ; $f05a: 85 01 VBLANK
	jmp $f072                ; $f05c: 4c 72 f0

; === Block $f05f-$f072 (Code) ===
loc_f05f:
	lda $8e                  ; $f05f: a5 8e
	sec                      ; $f061: 38
	sbc $89                  ; $f062: e5 89
	sta $02                  ; $f064: 85 02 WSYNC
	bpl $f072                ; $f066: 10 0a
loc_f068:
	ldy $91                  ; $f068: a4 91
	lda ($84),y              ; $f06a: b1 84
	sta $1c                  ; $f06c: 85 1c GRP1
	beq $f072                ; $f06e: f0 02
loc_f070:
	inc $91                  ; $f070: e6 91

; === Block $f072-$f0ba (Code) ===
loc_f072:
	ldx #$00                 ; $f072: a2 00 VSYNC
	lda $8e                  ; $f074: a5 8e
	sec                      ; $f076: 38
	sbc $87                  ; $f077: e5 87
	bpl $f084                ; $f079: 10 09
loc_f07b:
	ldy $90                  ; $f07b: a4 90
	lda ($82),y              ; $f07d: b1 82
	tax                      ; $f07f: aa
	beq $f084                ; $f080: f0 02
loc_f082:
	inc $90                  ; $f082: e6 90
loc_f084:
	ldy #$00                 ; $f084: a0 00 VSYNC
	lda $8e                  ; $f086: a5 8e
	sec                      ; $f088: 38
	sbc $8d                  ; $f089: e5 8d
	and #$fc                 ; $f08b: 29 fc
	bne $f091                ; $f08d: d0 02
loc_f08f:
	ldy #$02                 ; $f08f: a0 02 WSYNC
loc_f091:
	lda $8e                  ; $f091: a5 8e
	and #$0f                 ; $f093: 29 0f PF2
	bne $f0bd                ; $f095: d0 26
loc_f097:
	sta $02                  ; $f097: 85 02 WSYNC
	sty $1f                  ; $f099: 84 1f ENABL
	stx $1b                  ; $f09b: 86 1b GRP0
	ldy $8f                  ; $f09d: a4 8f
	lda ($80),y              ; $f09f: b1 80
	sta $0d                  ; $f0a1: 85 0d PF0
	iny                      ; $f0a3: c8
	lda ($80),y              ; $f0a4: b1 80
	sta $0e                  ; $f0a6: 85 0e PF1
	iny                      ; $f0a8: c8
	lda ($80),y              ; $f0a9: b1 80
	sta $0f                  ; $f0ab: 85 0f PF2
	iny                      ; $f0ad: c8
	sty $8f                  ; $f0ae: 84 8f
	dec $8e                  ; $f0b0: c6 8e
	lda $8e                  ; $f0b2: a5 8e
	cmp #$08                 ; $f0b4: c9 08 COLUPF
	bpl $f05f                ; $f0b6: 10 a7
loc_f0b8:
	sta $01                  ; $f0b8: 85 01 VBLANK
	jmp $f0c6                ; $f0ba: 4c c6 f0

; === Block $f0bd-$f0c3 (Code) ===
loc_f0bd:
	sta $02                  ; $f0bd: 85 02 WSYNC
	sty $1f                  ; $f0bf: 84 1f ENABL
	stx $1b                  ; $f0c1: 86 1b GRP0
	jmp $f0b0                ; $f0c3: 4c b0 f0

; === Block $f0c6-$f0d1 (Code) ===
loc_f0c6:
	lda #$00                 ; $f0c6: a9 00 VSYNC
	sta $1c                  ; $f0c8: 85 1c GRP1
	sta $1b                  ; $f0ca: 85 1b GRP0
	lda #$20                 ; $f0cc: a9 20 HMP0
	sta $0296                ; $f0ce: 8d 96 02 TIM64T
	rts                      ; $f0d1: 60

; === Block $f0d2-$f0eb (Code) ===
loc_f0d2:
	ldy #$02                 ; $f0d2: a0 02 WSYNC
	sec                      ; $f0d4: 38
	iny                      ; $f0d5: c8
	sbc #$0f                 ; $f0d6: e9 0f PF2
	bcs $f0d5                ; $f0d8: b0 fb
loc_f0da:
	eor #$ff                 ; $f0da: 49 ff
	sbc #$06                 ; $f0dc: e9 06 COLUP0
	asl                      ; $f0de: 0a
	asl                      ; $f0df: 0a
	asl                      ; $f0e0: 0a
	asl                      ; $f0e1: 0a
	sty $02                  ; $f0e2: 84 02 WSYNC
	dey                      ; $f0e4: 88
	bpl $f0e4                ; $f0e5: 10 fd
loc_f0e7:
	sta $10,x                ; $f0e7: 95 10 RESP0
	sta $20,x                ; $f0e9: 95 20 HMP0
	rts                      ; $f0eb: 60

; === Block $f0ec-$f10e (Code) ===
loc_f0ec:
	lda $0284                ; $f0ec: ad 84 02 INTIM
	bne $f0ec                ; $f0ef: d0 fb
loc_f0f1:
	lda #$02                 ; $f0f1: a9 02 WSYNC
	sta $02                  ; $f0f3: 85 02 WSYNC
	sta $01                  ; $f0f5: 85 01 VBLANK
	sta $02                  ; $f0f7: 85 02 WSYNC
	sta $02                  ; $f0f9: 85 02 WSYNC
	sta $02                  ; $f0fb: 85 02 WSYNC
	sta $00                  ; $f0fd: 85 00 VSYNC
	sta $02                  ; $f0ff: 85 02 WSYNC
	sta $02                  ; $f101: 85 02 WSYNC
	lda #$00                 ; $f103: a9 00 VSYNC
	sta $02                  ; $f105: 85 02 WSYNC
	sta $00                  ; $f107: 85 00 VSYNC
	lda #$2a                 ; $f109: a9 2a HMOVE
	sta $0296                ; $f10b: 8d 96 02 TIM64T
	rts                      ; $f10e: 60

; === Block $f10f-$f130 (Code) ===
loc_f10f:
	lda $8a                  ; $f10f: a5 8a
	jsr $f271                ; $f111: 20 71 f2
loc_f114:
	ldy #$00                 ; $f114: a0 00 VSYNC
	lda ($93),y              ; $f116: b1 93
	sta $80                  ; $f118: 85 80
	ldy #$01                 ; $f11a: a0 01 VBLANK
	lda ($93),y              ; $f11c: b1 93
	sta $81                  ; $f11e: 85 81
	lda $0282                ; $f120: ad 82 02 SWCHB
	and #$08                 ; $f123: 29 08 COLUPF
	beq $f133                ; $f125: f0 0c
loc_f127:
	ldy #$02                 ; $f127: a0 02 WSYNC
	lda ($93),y              ; $f129: b1 93
	jsr $f2d3                ; $f12b: 20 d3 f2
loc_f12e:
	sta $08                  ; $f12e: 85 08 COLUPF
	jmp $f13c                ; $f130: 4c 3c f1

; === Block $f133-$f1c2 (Code) ===
loc_f133:
	ldy #$03                 ; $f133: a0 03 RSYNC
	lda ($93),y              ; $f135: b1 93
	jsr $f2d3                ; $f137: 20 d3 f2
loc_f13a:
	sta $08                  ; $f13a: 85 08 COLUPF
loc_f13c:
	lda #$08                 ; $f13c: a9 08 COLUPF
	jsr $f2d3                ; $f13e: 20 d3 f2
loc_f141:
	sta $09                  ; $f141: 85 09 COLUBK
	ldy #$04                 ; $f143: a0 04 NUSIZ0
	lda ($93),y              ; $f145: b1 93
	sta $0a                  ; $f147: 85 0a CTRLPF
	and #$c0                 ; $f149: 29 c0
	lsr                      ; $f14b: 4a
	lsr                      ; $f14c: 4a
	lsr                      ; $f14d: 4a
	lsr                      ; $f14e: 4a
	lsr                      ; $f14f: 4a
	sta $1e                  ; $f150: 85 1e ENAM1
	lsr                      ; $f152: 4a
	sta $1d                  ; $f153: 85 1d ENAM0
	jsr $f235                ; $f155: 20 35 f2
loc_f158:
	lda $95                  ; $f158: a5 95
	cmp #$00                 ; $f15a: c9 00 VSYNC
	beq $f168                ; $f15c: f0 0a
loc_f15e:
	cmp #$5a                 ; $f15e: c9 5a AUDV1
	bne $f174                ; $f160: d0 12
loc_f162:
	lda $96                  ; $f162: a5 96
	cmp #$00                 ; $f164: c9 00 VSYNC
	beq $f174                ; $f166: f0 0c
loc_f168:
	lda $95                  ; $f168: a5 95
	sta $d8                  ; $f16a: 85 d8
	lda $96                  ; $f16c: a5 96
	sta $95                  ; $f16e: 85 95
	lda $d8                  ; $f170: a5 d8
	sta $96                  ; $f172: 85 96
loc_f174:
	ldx $95                  ; $f174: a6 95
	lda $ff44,x              ; $f176: bd 44 ff
	sta $93                  ; $f179: 85 93
	lda $ff45,x              ; $f17b: bd 45 ff
	sta $94                  ; $f17e: 85 94
	ldy #$01                 ; $f180: a0 01 VBLANK
	lda ($93),y              ; $f182: b1 93
	sta $86                  ; $f184: 85 86
	ldy #$02                 ; $f186: a0 02 WSYNC
	lda ($93),y              ; $f188: b1 93
	sta $87                  ; $f18a: 85 87
	lda $ff46,x              ; $f18c: bd 46 ff
	sta $93                  ; $f18f: 85 93
	lda $ff47,x              ; $f191: bd 47 ff
	sta $94                  ; $f194: 85 94
	ldy #$00                 ; $f196: a0 00 VSYNC
	lda ($93),y              ; $f198: b1 93
	sta $dc                  ; $f19a: 85 dc
	lda $ff48,x              ; $f19c: bd 48 ff
	sta $93                  ; $f19f: 85 93
	lda $ff49,x              ; $f1a1: bd 49 ff
	sta $94                  ; $f1a4: 85 94
	jsr $f2a1                ; $f1a6: 20 a1 f2
loc_f1a9:
	iny                      ; $f1a9: c8
	lda ($93),y              ; $f1aa: b1 93
	sta $82                  ; $f1ac: 85 82
	iny                      ; $f1ae: c8
	lda ($93),y              ; $f1af: b1 93
	sta $83                  ; $f1b1: 85 83
	lda $0282                ; $f1b3: ad 82 02 SWCHB
	and #$08                 ; $f1b6: 29 08 COLUPF
	beq $f1c5                ; $f1b8: f0 0b
loc_f1ba:
	lda $ff4a,x              ; $f1ba: bd 4a ff
	jsr $f2d3                ; $f1bd: 20 d3 f2
loc_f1c0:
	sta $06                  ; $f1c0: 85 06 COLUP0
	jmp $f1cd                ; $f1c2: 4c cd f1

; === Block $f1c5-$f222 (Code) ===
loc_f1c5:
	lda $ff4b,x              ; $f1c5: bd 4b ff
	jsr $f2d3                ; $f1c8: 20 d3 f2
loc_f1cb:
	sta $06                  ; $f1cb: 85 06 COLUP0
loc_f1cd:
	lda $ff4c,x              ; $f1cd: bd 4c ff
	ora #$10                 ; $f1d0: 09 10 RESP0
	sta $04                  ; $f1d2: 85 04 NUSIZ0
	ldx $96                  ; $f1d4: a6 96
	lda $ff44,x              ; $f1d6: bd 44 ff
	sta $93                  ; $f1d9: 85 93
	lda $ff45,x              ; $f1db: bd 45 ff
	sta $94                  ; $f1de: 85 94
	ldy #$01                 ; $f1e0: a0 01 VBLANK
	lda ($93),y              ; $f1e2: b1 93
	sta $88                  ; $f1e4: 85 88
	ldy #$02                 ; $f1e6: a0 02 WSYNC
	lda ($93),y              ; $f1e8: b1 93
	sta $89                  ; $f1ea: 85 89
	lda $ff46,x              ; $f1ec: bd 46 ff
	sta $93                  ; $f1ef: 85 93
	lda $ff47,x              ; $f1f1: bd 47 ff
	sta $94                  ; $f1f4: 85 94
	ldy #$00                 ; $f1f6: a0 00 VSYNC
	lda ($93),y              ; $f1f8: b1 93
	sta $dc                  ; $f1fa: 85 dc
	lda $ff48,x              ; $f1fc: bd 48 ff
	sta $93                  ; $f1ff: 85 93
	lda $ff49,x              ; $f201: bd 49 ff
	sta $94                  ; $f204: 85 94
	jsr $f2a1                ; $f206: 20 a1 f2
loc_f209:
	iny                      ; $f209: c8
	lda ($93),y              ; $f20a: b1 93
	sta $84                  ; $f20c: 85 84
	iny                      ; $f20e: c8
	lda ($93),y              ; $f20f: b1 93
	sta $85                  ; $f211: 85 85
	lda $0282                ; $f213: ad 82 02 SWCHB
	and #$08                 ; $f216: 29 08 COLUPF
	beq $f225                ; $f218: f0 0b
loc_f21a:
	lda $ff4a,x              ; $f21a: bd 4a ff
	jsr $f2d3                ; $f21d: 20 d3 f2
loc_f220:
	sta $07                  ; $f220: 85 07 COLUP1
	jmp $f22d                ; $f222: 4c 2d f2

; === Block $f225-$f234 (Code) ===
loc_f225:
	lda $ff4b,x              ; $f225: bd 4b ff
	jsr $f2d3                ; $f228: 20 d3 f2
loc_f22b:
	sta $07                  ; $f22b: 85 07 COLUP1
loc_f22d:
	lda $ff4c,x              ; $f22d: bd 4c ff
	ora #$10                 ; $f230: 09 10 RESP0
	sta $05                  ; $f232: 85 05 NUSIZ1
	rts                      ; $f234: 60

; === Block $f235-$f262 (Code) ===
loc_f235:
	ldy $9c                  ; $f235: a4 9c
	lda #$a2                 ; $f237: a9 a2
	sta $95                  ; $f239: 85 95
	sta $96                  ; $f23b: 85 96
	tya                      ; $f23d: 98
	clc                      ; $f23e: 18
	adc #$09                 ; $f23f: 69 09 COLUBK
	cmp #$a2                 ; $f241: c9 a2
	bcc $f247                ; $f243: 90 02
loc_f245:
	lda #$00                 ; $f245: a9 00 VSYNC
loc_f247:
	tay                      ; $f247: a8
	lda $ff44,y              ; $f248: b9 44 ff
	sta $93                  ; $f24b: 85 93
	lda $ff45,y              ; $f24d: b9 45 ff
	sta $94                  ; $f250: 85 94
	ldx #$00                 ; $f252: a2 00 VSYNC
	lda ($93,x)              ; $f254: a1 93
	cmp $8a                  ; $f256: c5 8a
	bne $f26a                ; $f258: d0 10
loc_f25a:
	lda $95                  ; $f25a: a5 95
	cmp #$a2                 ; $f25c: c9 a2
	bne $f265                ; $f25e: d0 05
loc_f260:
	sty $95                  ; $f260: 84 95
	jmp $f26a                ; $f262: 4c 6a f2

; === Block $f265-$f267 (Code) ===
loc_f265:
	sty $96                  ; $f265: 84 96
	jmp $f26e                ; $f267: 4c 6e f2

; === Block $f26a-$f270 (Code) ===
loc_f26a:
	cpy $9c                  ; $f26a: c4 9c
	bne $f23d                ; $f26c: d0 cf
loc_f26e:
	sty $9c                  ; $f26e: 84 9c
	rts                      ; $f270: 60

; === Block $f271-$f2a0 (Code) ===
loc_f271:
	sta $d8                  ; $f271: 85 d8
	sta $93                  ; $f273: 85 93
	lda #$00                 ; $f275: a9 00 VSYNC
	sta $94                  ; $f277: 85 94
	clc                      ; $f279: 18
	rol $93                  ; $f27a: 26 93
	rol $94                  ; $f27c: 26 94
	rol $93                  ; $f27e: 26 93
	rol $94                  ; $f280: 26 94
	rol $93                  ; $f282: 26 93
	rol $94                  ; $f284: 26 94
	lda $d8                  ; $f286: a5 d8
	clc                      ; $f288: 18
	adc $93                  ; $f289: 65 93
	sta $93                  ; $f28b: 85 93
	lda #$00                 ; $f28d: a9 00 VSYNC
	adc $94                  ; $f28f: 65 94
	sta $94                  ; $f291: 85 94
	lda #$1b                 ; $f293: a9 1b GRP0
	clc                      ; $f295: 18
	adc $93                  ; $f296: 65 93
	sta $93                  ; $f298: 85 93
	lda #$fe                 ; $f29a: a9 fe
	adc $94                  ; $f29c: 65 94
	sta $94                  ; $f29e: 85 94
	rts                      ; $f2a0: 60

; === Block $f2a1-$f2ae (Code) ===
loc_f2a1:
	ldy #$00                 ; $f2a1: a0 00 VSYNC
	lda $dc                  ; $f2a3: a5 dc
	cmp ($93),y              ; $f2a5: d1 93
	bcc $f2b1                ; $f2a7: 90 08
loc_f2a9:
	beq $f2b1                ; $f2a9: f0 06
loc_f2ab:
	iny                      ; $f2ab: c8
	iny                      ; $f2ac: c8
	iny                      ; $f2ad: c8
	jmp $f2a5                ; $f2ae: 4c a5 f2

; === Block $f2b1-$f2b1 (Code) ===
loc_f2b1:
	rts                      ; $f2b1: 60

; === Block $f2b2-$f2d2 (Code) ===
loc_f2b2:
	inc $e5                  ; $f2b2: e6 e5
	bne $f2be                ; $f2b4: d0 08
loc_f2b6:
	inc $e6                  ; $f2b6: e6 e6
	bne $f2be                ; $f2b8: d0 04
loc_f2ba:
	lda #$80                 ; $f2ba: a9 80
	sta $e6                  ; $f2bc: 85 e6
loc_f2be:
	lda $0280                ; $f2be: ad 80 02 SWCHA
	cmp #$ff                 ; $f2c1: c9 ff
	bne $f2ce                ; $f2c3: d0 09
loc_f2c5:
	lda $0282                ; $f2c5: ad 82 02 SWCHB
	and #$03                 ; $f2c8: 29 03 RSYNC
	cmp #$03                 ; $f2ca: c9 03 RSYNC
	beq $f2d2                ; $f2cc: f0 04
loc_f2ce:
	lda #$00                 ; $f2ce: a9 00 VSYNC
	sta $e6                  ; $f2d0: 85 e6
loc_f2d2:
	rts                      ; $f2d2: 60

; === Block $f2d3-$f2e3 (Code) ===
loc_f2d3:
	lsr                      ; $f2d3: 4a
	bcc $f2da                ; $f2d4: 90 04
loc_f2d6:
	tay                      ; $f2d6: a8
	lda $0080,y              ; $f2d7: b9 80 00
loc_f2da:
	ldy $e6                  ; $f2da: a4 e6
	bpl $f2e2                ; $f2dc: 10 04
loc_f2de:
	eor $e6                  ; $f2de: 45 e6
	and #$fb                 ; $f2e0: 29 fb
loc_f2e2:
	asl                      ; $f2e2: 0a
	rts                      ; $f2e3: 60

; === Block $f2e4-$f2ee (Code) ===
loc_f2e4:
	lda $ff44,x              ; $f2e4: bd 44 ff
	sta $93                  ; $f2e7: 85 93
	lda $ff45,x              ; $f2e9: bd 45 ff
	sta $94                  ; $f2ec: 85 94
	rts                      ; $f2ee: 60

; === Block $f2ef-$f362 (Code) ===
loc_f2ef:
	sei                      ; $f2ef: 78
	cld                      ; $f2f0: d8
	ldx #$28                 ; $f2f1: a2 28 RESMP0
	lda #$00                 ; $f2f3: a9 00 VSYNC
	sta $04,x                ; $f2f5: 95 04 NUSIZ0
	dex                      ; $f2f7: ca
	bpl $f2f5                ; $f2f8: 10 fb
loc_f2fa:
	txs                      ; $f2fa: 9a
	sta $00,x                ; $f2fb: 95 00 VSYNC
	dex                      ; $f2fd: ca
	bmi $f2fb                ; $f2fe: 30 fb
loc_f300:
	jsr $f371                ; $f300: 20 71 f3
loc_f303:
	jsr $f3d3                ; $f303: 20 d3 f3
loc_f306:
	jsr $f384                ; $f306: 20 84 f3
loc_f309:
	jsr $fa23                ; $f309: 20 23 fa
loc_f30c:
	jsr $f2b2                ; $f30c: 20 b2 f2
loc_f30f:
	lda $de                  ; $f30f: a5 de
	bne $f365                ; $f311: d0 52
loc_f313:
	lda $b9                  ; $f313: a5 b9
	cmp #$12                 ; $f315: c9 12 RESM0
	bne $f323                ; $f317: d0 0a
loc_f319:
	lda #$ff                 ; $f319: a9 ff
	sta $df                  ; $f31b: 85 df
	sta $de                  ; $f31d: 85 de
	lda #$00                 ; $f31f: a9 00 VSYNC
	sta $e0                  ; $f321: 85 e0
loc_f323:
	ldy #$00                 ; $f323: a0 00 VSYNC
	jsr $f4c2                ; $f325: 20 c2 f4
loc_f328:
	jsr $f5d4                ; $f328: 20 d4 f5
loc_f32b:
	jsr $f0ec                ; $f32b: 20 ec f0
loc_f32e:
	jsr $f10f                ; $f32e: 20 0f f1
loc_f331:
	jsr $f008                ; $f331: 20 08 f0
loc_f334:
	jsr $f556                ; $f334: 20 56 f5
loc_f337:
	ldy #$01                 ; $f337: a0 01 VBLANK
	jsr $f4c2                ; $f339: 20 c2 f4
loc_f33c:
	jsr $f9e7                ; $f33c: 20 e7 f9
loc_f33f:
	jsr $f0ec                ; $f33f: 20 ec f0
loc_f342:
	jsr $f8a5                ; $f342: 20 a5 f8
loc_f345:
	jsr $f93c                ; $f345: 20 3c f9
loc_f348:
	jsr $f008                ; $f348: 20 08 f0
loc_f34b:
	jsr $f7cb                ; $f34b: 20 cb f7
loc_f34e:
	jsr $f7b0                ; $f34e: 20 b0 f7
loc_f351:
	jsr $f0ec                ; $f351: 20 ec f0
loc_f354:
	ldy #$02                 ; $f354: a0 02 WSYNC
	jsr $f4c2                ; $f356: 20 c2 f4
loc_f359:
	jsr $f795                ; $f359: 20 95 f7
loc_f35c:
	jsr $f9b3                ; $f35c: 20 b3 f9
loc_f35f:
	jsr $f008                ; $f35f: 20 08 f0
loc_f362:
	jmp $f306                ; $f362: 4c 06 f3

; === Block $f365-$f36e (Code) ===
loc_f365:
	jsr $f0ec                ; $f365: 20 ec f0
loc_f368:
	jsr $f008                ; $f368: 20 08 f0
loc_f36b:
	jsr $f10f                ; $f36b: 20 0f f1
loc_f36e:
	jmp $f306                ; $f36e: 4c 06 f3

; === Block $f371-$f383 (Code) ===
loc_f371:
	lda #$0d                 ; $f371: a9 0d PF0
	ldx #$02                 ; $f373: a2 02 WSYNC
	jsr $f0d2                ; $f375: 20 d2 f0
loc_f378:
	lda #$96                 ; $f378: a9 96
	ldx #$03                 ; $f37a: a2 03 RSYNC
	jsr $f0d2                ; $f37c: 20 d2 f0
loc_f37f:
	sta $02                  ; $f37f: 85 02 WSYNC
	sta $2a                  ; $f381: 85 2a HMOVE
	rts                      ; $f383: 60

; === Block $f384-$f3d3 (Code) ===
loc_f384:
	lda $0282                ; $f384: ad 82 02 SWCHB
	eor #$ff                 ; $f387: 49 ff
	and $92                  ; $f389: 25 92
	and #$01                 ; $f38b: 29 01 VBLANK
	beq $f3b5                ; $f38d: f0 26
loc_f38f:
	lda $de                  ; $f38f: a5 de
	cmp #$ff                 ; $f391: c9 ff
	beq $f3d3                ; $f393: f0 3e
loc_f395:
	lda #$11                 ; $f395: a9 11 RESP1
	sta $8a                  ; $f397: 85 8a
	sta $e2                  ; $f399: 85 e2
	lda #$50                 ; $f39b: a9 50 RESP0
	sta $8b                  ; $f39d: 85 8b
	sta $e3                  ; $f39f: 85 e3
	lda #$20                 ; $f3a1: a9 20 HMP0
	sta $8c                  ; $f3a3: 85 8c
	sta $e4                  ; $f3a5: 85 e4
	lda #$00                 ; $f3a7: a9 00 VSYNC
	sta $a8                  ; $f3a9: 85 a8
	sta $ad                  ; $f3ab: 85 ad
	sta $b2                  ; $f3ad: 85 b2
	sta $df                  ; $f3af: 85 df
	lda #$a2                 ; $f3b1: a9 a2
	sta $9d                  ; $f3b3: 85 9d
loc_f3b5:
	lda $0282                ; $f3b5: ad 82 02 SWCHB
	eor #$ff                 ; $f3b8: 49 ff
	and $92                  ; $f3ba: 25 92
	and #$02                 ; $f3bc: 29 02 WSYNC
	beq $f40c                ; $f3be: f0 4c
loc_f3c0:
	lda $8a                  ; $f3c0: a5 8a
	cmp #$00                 ; $f3c2: c9 00 VSYNC
	bne $f3d3                ; $f3c4: d0 0d
loc_f3c6:
	lda $dd                  ; $f3c6: a5 dd
	clc                      ; $f3c8: 18
	adc #$02                 ; $f3c9: 69 02 WSYNC
	cmp #$06                 ; $f3cb: c9 06 COLUP0
	bcc $f3d1                ; $f3cd: 90 02
loc_f3cf:
	lda #$00                 ; $f3cf: a9 00 VSYNC
loc_f3d1:
	sta $dd                  ; $f3d1: 85 dd

; === Block $f3d3-$f411 (Code) ===
loc_f3d3:
	lda #$00                 ; $f3d3: a9 00 VSYNC
	sta $8a                  ; $f3d5: 85 8a
	sta $e2                  ; $f3d7: 85 e2
	lda #$00                 ; $f3d9: a9 00 VSYNC
	sta $8c                  ; $f3db: 85 8c
	sta $e4                  ; $f3dd: 85 e4
	ldy $dd                  ; $f3df: a4 dd
	lda $f45a,y              ; $f3e1: b9 5a f4
	sta $93                  ; $f3e4: 85 93
	lda $f45b,y              ; $f3e6: b9 5b f4
	sta $94                  ; $f3e9: 85 94
	ldy #$30                 ; $f3eb: a0 30
	lda ($93),y              ; $f3ed: b1 93
	sta $00a1,y              ; $f3ef: 99 a1 00
	dey                      ; $f3f2: 88
	bpl $f3ed                ; $f3f3: 10 f8
loc_f3f5:
	lda $dd                  ; $f3f5: a5 dd
	cmp #$04                 ; $f3f7: c9 04 NUSIZ0
	bcc $f404                ; $f3f9: 90 09
loc_f3fb:
	jsr $f412                ; $f3fb: 20 12 f4
loc_f3fe:
	jsr $f0ec                ; $f3fe: 20 ec f0
loc_f401:
	jsr $f008                ; $f401: 20 08 f0
loc_f404:
	lda #$00                 ; $f404: a9 00 VSYNC
	sta $de                  ; $f406: 85 de
	lda #$a2                 ; $f408: a9 a2
	sta $9d                  ; $f40a: 85 9d
	lda $0282                ; $f40c: ad 82 02 SWCHB
	sta $92                  ; $f40f: 85 92
	rts                      ; $f411: 60

; === Block $f412-$f438 (Code) ===
loc_f412:
	ldy #$1e                 ; $f412: a0 1e ENAM1
	lda $e5                  ; $f414: a5 e5
	lsr                      ; $f416: 4a
	lsr                      ; $f417: 4a
	lsr                      ; $f418: 4a
	lsr                      ; $f419: 4a
	lsr                      ; $f41a: 4a
	sec                      ; $f41b: 38
	adc $e5                  ; $f41c: 65 e5
	sta $e5                  ; $f41e: 85 e5
	and #$1f                 ; $f420: 29 1f ENABL
	cmp $f43a,y              ; $f422: d9 3a f4
	bcc $f414                ; $f425: 90 ed
loc_f427:
	cmp $f43b,y              ; $f427: d9 3b f4
	beq $f42e                ; $f42a: f0 02
loc_f42c:
	bcs $f414                ; $f42c: b0 e6
loc_f42e:
	ldx $f439,y              ; $f42e: be 39 f4
	sta $00,x                ; $f431: 95 00 VSYNC
	dey                      ; $f433: 88
	dey                      ; $f434: 88
	dey                      ; $f435: 88
	bpl $f414                ; $f436: 10 dc
loc_f438:
	rts                      ; $f438: 60

; === Block $f4c2-$f4f2 (Code) ===
loc_f4c2:
	lda $36                  ; $f4c2: a5 36
	and #$80                 ; $f4c4: 29 80
	nop                      ; $f4c6: ea
	nop                      ; $f4c7: ea
	lda $34                  ; $f4c8: a5 34
	and #$40                 ; $f4ca: 29 40 VSYNC
	bne $f4f5                ; $f4cc: d0 27
loc_f4ce:
	lda $35                  ; $f4ce: a5 35
	and #$40                 ; $f4d0: 29 40 VSYNC
	beq $f4da                ; $f4d2: f0 06
loc_f4d4:
	lda $96                  ; $f4d4: a5 96
	cmp #$87                 ; $f4d6: c9 87
	bne $f4f5                ; $f4d8: d0 1b
loc_f4da:
	lda $32                  ; $f4da: a5 32
	and #$40                 ; $f4dc: 29 40 VSYNC
	beq $f4e6                ; $f4de: f0 06
loc_f4e0:
	lda $95                  ; $f4e0: a5 95
	cmp #$00                 ; $f4e2: c9 00 VSYNC
	bne $f4f5                ; $f4e4: d0 0f
loc_f4e6:
	lda $33                  ; $f4e6: a5 33
	and #$40                 ; $f4e8: 29 40 VSYNC
	beq $f51f                ; $f4ea: f0 33
loc_f4ec:
	lda $96                  ; $f4ec: a5 96
	cmp #$00                 ; $f4ee: c9 00 VSYNC
	bne $f4f5                ; $f4f0: d0 03
loc_f4f2:
	jmp $f51f                ; $f4f2: 4c 1f f5

; === Block $f4f5-$f552 (Code) ===
loc_f4f5:
	cpy #$02                 ; $f4f5: c0 02 WSYNC
	bne $f52f                ; $f4f7: d0 36
loc_f4f9:
	lda $9d                  ; $f4f9: a5 9d
	cmp #$5a                 ; $f4fb: c9 5a AUDV1
	beq $f52f                ; $f4fd: f0 30
loc_f4ff:
	lda $8a                  ; $f4ff: a5 8a
	cmp $bc                  ; $f501: c5 bc
	bne $f52f                ; $f503: d0 2a
loc_f505:
	lda $8b                  ; $f505: a5 8b
	sec                      ; $f507: 38
	sbc $bd                  ; $f508: e5 bd
	cmp #$0a                 ; $f50a: c9 0a CTRLPF
	bcc $f52f                ; $f50c: 90 21
loc_f50e:
	cmp #$17                 ; $f50e: c9 17 AUDF0
	bcs $f52f                ; $f510: b0 1d
loc_f512:
	lda $be                  ; $f512: a5 be
	sec                      ; $f514: 38
	sbc $8c                  ; $f515: e5 8c
	cmp #$fc                 ; $f517: c9 fc
	bcs $f51f                ; $f519: b0 04
loc_f51b:
	cmp #$19                 ; $f51b: c9 19 AUDV0
	bcs $f52f                ; $f51d: b0 10
loc_f51f:
	lda #$ff                 ; $f51f: a9 ff
	sta $99                  ; $f521: 85 99
	lda $8a                  ; $f523: a5 8a
	sta $e2                  ; $f525: 85 e2
	lda $8b                  ; $f527: a5 8b
	sta $e3                  ; $f529: 85 e3
	lda $8c                  ; $f52b: a5 8c
	sta $e4                  ; $f52d: 85 e4
loc_f52f:
	cpy #$00                 ; $f52f: c0 00 VSYNC
	bne $f538                ; $f531: d0 05
loc_f533:
	lda $0280                ; $f533: ad 80 02 SWCHA
	sta $99                  ; $f536: 85 99
loc_f538:
	lda $e2                  ; $f538: a5 e2
	sta $8a                  ; $f53a: 85 8a
	lda $e3                  ; $f53c: a5 e3
	sta $8b                  ; $f53e: 85 8b
	lda $e4                  ; $f540: a5 e4
	sta $8c                  ; $f542: 85 8c
	lda $99                  ; $f544: a5 99
	ora $f553,y              ; $f546: 19 53 f5
	sta $9b                  ; $f549: 85 9b
	ldy #$03                 ; $f54b: a0 03 RSYNC
	ldx #$8a                 ; $f54d: a2 8a
	jsr $f5ff                ; $f54f: 20 ff f5
loc_f552:
	rts                      ; $f552: 60

; === Block $f556-$f580 (Code) ===
loc_f556:
	rol $3c                  ; $f556: 26 3c
	ror $d7                  ; $f558: 66 d7
	lda $d7                  ; $f55a: a5 d7
	and #$c0                 ; $f55c: 29 c0
	cmp #$40                 ; $f55e: c9 40 VSYNC
	bne $f572                ; $f560: d0 10
loc_f562:
	lda #$a2                 ; $f562: a9 a2
	cmp $9d                  ; $f564: c5 9d
	beq $f572                ; $f566: f0 0a
loc_f568:
	sta $9d                  ; $f568: 85 9d
	lda #$04                 ; $f56a: a9 04 NUSIZ0
	sta $e0                  ; $f56c: 85 e0
	lda #$04                 ; $f56e: a9 04 NUSIZ0
	sta $df                  ; $f570: 85 df
loc_f572:
	lda #$ff                 ; $f572: a9 ff
	sta $98                  ; $f574: 85 98
	lda $32                  ; $f576: a5 32
	and #$40                 ; $f578: 29 40 VSYNC
	beq $f583                ; $f57a: f0 07
loc_f57c:
	lda $95                  ; $f57c: a5 95
	sta $97                  ; $f57e: 85 97
	jmp $f593                ; $f580: 4c 93 f5

; === Block $f583-$f58d (Code) ===
loc_f583:
	lda $33                  ; $f583: a5 33
	and #$40                 ; $f585: 29 40 VSYNC
	beq $f590                ; $f587: f0 07
loc_f589:
	lda $96                  ; $f589: a5 96
	sta $97                  ; $f58b: 85 97
	jmp $f593                ; $f58d: 4c 93 f5

; === Block $f590-$f590 (Code) ===
loc_f590:
	jmp $f5d3                ; $f590: 4c d3 f5

; === Block $f593-$f5d3 (Code) ===
loc_f593:
	ldx $97                  ; $f593: a6 97
	jsr $f2e4                ; $f595: 20 e4 f2
loc_f598:
	lda $97                  ; $f598: a5 97
	cmp #$51                 ; $f59a: c9 51 RESP1
	bcc $f5d3                ; $f59c: 90 35
loc_f59e:
	ldy #$00                 ; $f59e: a0 00 VSYNC
	lda ($93),y              ; $f5a0: b1 93
	cmp $8a                  ; $f5a2: c5 8a
	bne $f5d3                ; $f5a4: d0 2d
loc_f5a6:
	lda $97                  ; $f5a6: a5 97
	cmp $9d                  ; $f5a8: c5 9d
	beq $f5b4                ; $f5aa: f0 08
loc_f5ac:
	lda #$05                 ; $f5ac: a9 05 NUSIZ1
	sta $e0                  ; $f5ae: 85 e0
	lda #$04                 ; $f5b0: a9 04 NUSIZ0
	sta $df                  ; $f5b2: 85 df
loc_f5b4:
	lda $97                  ; $f5b4: a5 97
	sta $9d                  ; $f5b6: 85 9d
	ldx $93                  ; $f5b8: a6 93
	ldy #$06                 ; $f5ba: a0 06 COLUP0
	lda $99                  ; $f5bc: a5 99
	jsr $f6ac                ; $f5be: 20 ac f6
loc_f5c1:
	ldy #$01                 ; $f5c1: a0 01 VBLANK
	lda ($93),y              ; $f5c3: b1 93
	sec                      ; $f5c5: 38
	sbc $8b                  ; $f5c6: e5 8b
	sta $9e                  ; $f5c8: 85 9e
	ldy #$02                 ; $f5ca: a0 02 WSYNC
	lda ($93),y              ; $f5cc: b1 93
	sec                      ; $f5ce: 38
	sbc $8c                  ; $f5cf: e5 8c
	sta $9f                  ; $f5d1: 85 9f
loc_f5d3:
	rts                      ; $f5d3: 60

; === Block $f5d4-$f5fe (Code) ===
loc_f5d4:
	ldx $9d                  ; $f5d4: a6 9d
	cpx #$a2                 ; $f5d6: e0 a2
	beq $f5fe                ; $f5d8: f0 24
loc_f5da:
	jsr $f2e4                ; $f5da: 20 e4 f2
loc_f5dd:
	ldy #$00                 ; $f5dd: a0 00 VSYNC
	lda $8a                  ; $f5df: a5 8a
	sta ($93),y              ; $f5e1: 91 93
	ldy #$01                 ; $f5e3: a0 01 VBLANK
	lda $8b                  ; $f5e5: a5 8b
	clc                      ; $f5e7: 18
	adc $9e                  ; $f5e8: 65 9e
	sta ($93),y              ; $f5ea: 91 93
	ldy #$02                 ; $f5ec: a0 02 WSYNC
	lda $8c                  ; $f5ee: a5 8c
	clc                      ; $f5f0: 18
	adc $9f                  ; $f5f1: 65 9f
	sta ($93),y              ; $f5f3: 91 93
	ldy #$00                 ; $f5f5: a0 00 VSYNC
	lda #$ff                 ; $f5f7: a9 ff
	ldx $93                  ; $f5f9: a6 93
	jsr $f5ff                ; $f5fb: 20 ff f5
loc_f5fe:
	rts                      ; $f5fe: 60

; === Block $f5ff-$f62e (Code) ===
loc_f5ff:
	jsr $f6ac                ; $f5ff: 20 ac f6
loc_f602:
	ldy #$02                 ; $f602: a0 02 WSYNC
	sty $9a                  ; $f604: 84 9a
	lda $00c8,y              ; $f606: b9 c8 00
	cmp #$1c                 ; $f609: c9 1c GRP1
	beq $f62f                ; $f60b: f0 22
loc_f60d:
	ldy $9a                  ; $f60d: a4 9a
	lda $00,x                ; $f60f: b5 00 VSYNC
	cmp $f9ad,y              ; $f611: d9 ad f9
	bne $f62f                ; $f614: d0 19
loc_f616:
	lda $02,x                ; $f616: b5 02 WSYNC
	cmp #$0d                 ; $f618: c9 0d PF0
	bpl $f62f                ; $f61a: 10 13
loc_f61c:
	lda $f9b0,y              ; $f61c: b9 b0 f9
	sta $00,x                ; $f61f: 95 00 VSYNC
	lda #$50                 ; $f621: a9 50 RESP0
	sta $01,x                ; $f623: 95 01 VBLANK
	lda #$2c                 ; $f625: a9 2c CXCLR
	sta $02,x                ; $f627: 95 02 WSYNC
	lda #$01                 ; $f629: a9 01 VBLANK
	sta $00c8,y              ; $f62b: 99 c8 00
	rts                      ; $f62e: 60

; === Block $f62f-$f640 (Code) ===
loc_f62f:
	ldy $9a                  ; $f62f: a4 9a
	dey                      ; $f631: 88
	bpl $f604                ; $f632: 10 d0
loc_f634:
	lda $02,x                ; $f634: b5 02 WSYNC
	cmp #$6a                 ; $f636: c9 6a HMOVE
	bmi $f643                ; $f638: 30 09
loc_f63a:
	lda #$0d                 ; $f63a: a9 0d PF0
	sta $02,x                ; $f63c: 95 02 WSYNC
	ldy #$05                 ; $f63e: a0 05 NUSIZ1
	jmp $f69f                ; $f640: 4c 9f f6

; === Block $f643-$f64d (Code) ===
loc_f643:
	lda $01,x                ; $f643: b5 01 VBLANK
	cmp #$03                 ; $f645: c9 03 RSYNC
	bcc $f650                ; $f647: 90 07
loc_f649:
	cmp #$f0                 ; $f649: c9 f0
	bcs $f650                ; $f64b: b0 03
loc_f64d:
	jmp $f662                ; $f64d: 4c 62 f6

; === Block $f650-$f656 (Code) ===
loc_f650:
	cpx #$8a                 ; $f650: e0 8a
	beq $f659                ; $f652: f0 05
loc_f654:
	lda #$9a                 ; $f654: a9 9a
	jmp $f65b                ; $f656: 4c 5b f6

; === Block $f659-$f65f (Code) ===
loc_f659:
	lda #$9e                 ; $f659: a9 9e
loc_f65b:
	sta $01,x                ; $f65b: 95 01 VBLANK
	ldy #$08                 ; $f65d: a0 08 COLUPF
	jmp $f69f                ; $f65f: 4c 9f f6

; === Block $f662-$f66e (Code) ===
loc_f662:
	lda $02,x                ; $f662: b5 02 WSYNC
	cmp #$0d                 ; $f664: c9 0d PF0
	bcs $f671                ; $f666: b0 09
loc_f668:
	lda #$69                 ; $f668: a9 69 RESMP1
	sta $02,x                ; $f66a: 95 02 WSYNC
	ldy #$07                 ; $f66c: a0 07 COLUP1
	jmp $f69f                ; $f66e: 4c 9f f6

; === Block $f671-$f68f (Code) ===
loc_f671:
	lda $01,x                ; $f671: b5 01 VBLANK
	cpx #$8a                 ; $f673: e0 8a
	bne $f692                ; $f675: d0 1b
loc_f677:
	cmp #$9f                 ; $f677: c9 9f
	bcc $f6ab                ; $f679: 90 30
loc_f67b:
	lda $00,x                ; $f67b: b5 00 VSYNC
	cmp #$03                 ; $f67d: c9 03 RSYNC
	bne $f696                ; $f67f: d0 15
loc_f681:
	lda $a1                  ; $f681: a5 a1
	cmp #$15                 ; $f683: c9 15 AUDC0
	beq $f696                ; $f685: f0 0f
loc_f687:
	lda #$1e                 ; $f687: a9 1e ENAM1
	sta $00,x                ; $f689: 95 00 VSYNC
	lda #$03                 ; $f68b: a9 03 RSYNC
	sta $01,x                ; $f68d: 95 01 VBLANK
	jmp $f6ab                ; $f68f: 4c ab f6

; === Block $f692-$f69c (Code) ===
loc_f692:
	cmp #$9b                 ; $f692: c9 9b
	bcc $f6ab                ; $f694: 90 15
loc_f696:
	lda #$03                 ; $f696: a9 03 RSYNC
	sta $01,x                ; $f698: 95 01 VBLANK
	ldy #$06                 ; $f69a: a0 06 COLUP0
	jmp $f69f                ; $f69c: 4c 9f f6

; === Block $f69f-$f6ab (Code) ===
loc_f69f:
	lda $00,x                ; $f69f: b5 00 VSYNC
	jsr $f271                ; $f6a1: 20 71 f2
loc_f6a4:
	lda ($93),y              ; $f6a4: b1 93
	jsr $f6d5                ; $f6a6: 20 d5 f6
loc_f6a9:
	sta $00,x                ; $f6a9: 95 00 VSYNC
	rts                      ; $f6ab: 60

; === Block $f6ac-$f6d1 (Code) ===
loc_f6ac:
	sta $9b                  ; $f6ac: 85 9b
	dey                      ; $f6ae: 88
	bmi $f6d4                ; $f6af: 30 23
loc_f6b1:
	lda $9b                  ; $f6b1: a5 9b
	and #$80                 ; $f6b3: 29 80
	bne $f6b9                ; $f6b5: d0 02
loc_f6b7:
	inc $01,x                ; $f6b7: f6 01 VBLANK
loc_f6b9:
	lda $9b                  ; $f6b9: a5 9b
	and #$40                 ; $f6bb: 29 40 VSYNC
	bne $f6c1                ; $f6bd: d0 02
loc_f6bf:
	dec $01,x                ; $f6bf: d6 01 VBLANK
loc_f6c1:
	lda $9b                  ; $f6c1: a5 9b
	and #$10                 ; $f6c3: 29 10 RESP0
	bne $f6c9                ; $f6c5: d0 02
loc_f6c7:
	inc $02,x                ; $f6c7: f6 02 WSYNC
loc_f6c9:
	lda $9b                  ; $f6c9: a5 9b
	and #$20                 ; $f6cb: 29 20 HMP0
	bne $f6d1                ; $f6cd: d0 02
loc_f6cf:
	dec $02,x                ; $f6cf: d6 02 WSYNC
loc_f6d1:
	jmp $f6ae                ; $f6d1: 4c ae f6

; === Block $f6d4-$f6d4 (Code) ===
loc_f6d4:
	rts                      ; $f6d4: 60

; === Block $f6d5-$f6e8 (Code) ===
loc_f6d5:
	cmp #$80                 ; $f6d5: c9 80
	bcc $f6e8                ; $f6d7: 90 0f
loc_f6d9:
	sec                      ; $f6d9: 38
	sbc #$80                 ; $f6da: e9 80
	sta $d8                  ; $f6dc: 85 d8
	lda $dd                  ; $f6de: a5 dd
	lsr                      ; $f6e0: 4a
	clc                      ; $f6e1: 18
	adc $d8                  ; $f6e2: 65 d8
	tay                      ; $f6e4: a8
	lda $ff32,y              ; $f6e5: b9 32 ff
loc_f6e8:
	rts                      ; $f6e8: 60

; === Block $f6e9-$f6f3 (Code) ===
loc_f6e9:
	cmp $95                  ; $f6e9: c5 95
	beq $f6f4                ; $f6eb: f0 07
loc_f6ed:
	cmp $96                  ; $f6ed: c5 96
	beq $f6f9                ; $f6ef: f0 08
loc_f6f1:
	lda #$00                 ; $f6f1: a9 00 VSYNC
	rts                      ; $f6f3: 60

; === Block $f6f4-$f6f8 (Code) ===
loc_f6f4:
	lda $32                  ; $f6f4: a5 32
	and #$40                 ; $f6f6: 29 40 VSYNC
	rts                      ; $f6f8: 60

; === Block $f6f9-$f6fd (Code) ===
loc_f6f9:
	lda $33                  ; $f6f9: a5 33
	and #$40                 ; $f6fb: 29 40 VSYNC
	rts                      ; $f6fd: 60

; === Block $f6fe-$f70e (Code) ===
loc_f6fe:
	lda $37                  ; $f6fe: a5 37
	and #$80                 ; $f700: 29 80
	beq $f70c                ; $f702: f0 08
loc_f704:
	cpx $95                  ; $f704: e4 95
	beq $f70f                ; $f706: f0 07
loc_f708:
	cpx $96                  ; $f708: e4 96
	beq $f712                ; $f70a: f0 06
loc_f70c:
	lda #$a2                 ; $f70c: a9 a2
	rts                      ; $f70e: 60

; === Block $f70f-$f711 (Code) ===
loc_f70f:
	lda $96                  ; $f70f: a5 96
	rts                      ; $f711: 60

; === Block $f712-$f714 (Code) ===
loc_f712:
	lda $95                  ; $f712: a5 95
	rts                      ; $f714: 60

; === Block $f715-$f727 (Code) ===
loc_f715:
	jsr $f728                ; $f715: 20 28 f7
loc_f718:
	ldx $d5                  ; $f718: a6 d5
	lda $9b                  ; $f71a: a5 9b
	bne $f720                ; $f71c: d0 02
loc_f71e:
	lda $03,x                ; $f71e: b5 03 RSYNC
loc_f720:
	sta $03,x                ; $f720: 95 03 RSYNC
	ldy $d4                  ; $f722: a4 d4
	jsr $f5ff                ; $f724: 20 ff f5
loc_f727:
	rts                      ; $f727: 60

; === Block $f728-$f747 (Code) ===
loc_f728:
	lda #$00                 ; $f728: a9 00 VSYNC
	sta $e1                  ; $f72a: 85 e1
	ldy $e1                  ; $f72c: a4 e1
	lda ($d2),y              ; $f72e: b1 d2
	tax                      ; $f730: aa
	iny                      ; $f731: c8
	lda ($d2),y              ; $f732: b1 d2
	tay                      ; $f734: a8
	lda $00,x                ; $f735: b5 00 VSYNC
	cmp $0000,y              ; $f737: d9 00 00 VSYNC
	bne $f748                ; $f73a: d0 0c
loc_f73c:
	cpy $d6                  ; $f73c: c4 d6
	beq $f748                ; $f73e: f0 08
loc_f740:
	cpx $d6                  ; $f740: e4 d6
	beq $f748                ; $f742: f0 04
loc_f744:
	jsr $f757                ; $f744: 20 57 f7
loc_f747:
	rts                      ; $f747: 60

; === Block $f748-$f756 (Code) ===
loc_f748:
	inc $e1                  ; $f748: e6 e1
	inc $e1                  ; $f74a: e6 e1
	ldy $e1                  ; $f74c: a4 e1
	lda ($d2),y              ; $f74e: b1 d2
	bne $f72c                ; $f750: d0 da
loc_f752:
	lda #$00                 ; $f752: a9 00 VSYNC
	sta $9b                  ; $f754: 85 9b
	rts                      ; $f756: 60

; === Block $f757-$f771 (Code) ===
loc_f757:
	lda #$ff                 ; $f757: a9 ff
	sta $9b                  ; $f759: 85 9b
	lda $0000,y              ; $f75b: b9 00 00 VSYNC
	cmp $00,x                ; $f75e: d5 00 VSYNC
	bne $f792                ; $f760: d0 30
loc_f762:
	lda $0001,y              ; $f762: b9 01 00 VBLANK
	cmp $01,x                ; $f765: d5 01 VBLANK
	bcc $f774                ; $f767: 90 0b
loc_f769:
	beq $f77a                ; $f769: f0 0f
loc_f76b:
	lda $9b                  ; $f76b: a5 9b
	and #$7f                 ; $f76d: 29 7f
	sta $9b                  ; $f76f: 85 9b
	jmp $f77a                ; $f771: 4c 7a f7

; === Block $f774-$f789 (Code) ===
loc_f774:
	lda $9b                  ; $f774: a5 9b
	and #$bf                 ; $f776: 29 bf
	sta $9b                  ; $f778: 85 9b
loc_f77a:
	lda $0002,y              ; $f77a: b9 02 00 WSYNC
	cmp $02,x                ; $f77d: d5 02 WSYNC
	bcc $f78c                ; $f77f: 90 0b
loc_f781:
	beq $f792                ; $f781: f0 0f
loc_f783:
	lda $9b                  ; $f783: a5 9b
	and #$ef                 ; $f785: 29 ef
	sta $9b                  ; $f787: 85 9b
	jmp $f792                ; $f789: 4c 92 f7

; === Block $f78c-$f792 (Code) ===
loc_f78c:
	lda $9b                  ; $f78c: a5 9b
	and #$df                 ; $f78e: 29 df
	sta $9b                  ; $f790: 85 9b

; === Block $f792-$f794 (Code) ===
loc_f792:
	lda $9b                  ; $f792: a5 9b
	rts                      ; $f794: 60

; === Block $f795-$f7a6 (Code) ===
loc_f795:
	lda #$a7                 ; $f795: a9 a7
	sta $d2                  ; $f797: 85 d2
	lda #$f7                 ; $f799: a9 f7
	sta $d3                  ; $f79b: 85 d3
	lda #$03                 ; $f79d: a9 03 RSYNC
	sta $d4                  ; $f79f: 85 d4
	ldx #$36                 ; $f7a1: a2 36
	jsr $f7ea                ; $f7a3: 20 ea f7
loc_f7a6:
	rts                      ; $f7a6: 60

; === Block $f7b0-$f7c1 (Code) ===
loc_f7b0:
	lda #$c2                 ; $f7b0: a9 c2
	sta $d2                  ; $f7b2: 85 d2
	lda #$f7                 ; $f7b4: a9 f7
	sta $d3                  ; $f7b6: 85 d3
	lda #$02                 ; $f7b8: a9 02 WSYNC
	sta $d4                  ; $f7ba: 85 d4
	ldx #$3f                 ; $f7bc: a2 3f
	jsr $f7ea                ; $f7be: 20 ea f7
loc_f7c1:
	rts                      ; $f7c1: 60

; === Block $f7cb-$f7dc (Code) ===
loc_f7cb:
	lda #$dd                 ; $f7cb: a9 dd
	sta $d2                  ; $f7cd: 85 d2
	lda #$f7                 ; $f7cf: a9 f7
	sta $d3                  ; $f7d1: 85 d3
	lda #$02                 ; $f7d3: a9 02 WSYNC
	sta $d4                  ; $f7d5: 85 d4
	ldx #$48                 ; $f7d7: a2 48 COLUPF
	jsr $f7ea                ; $f7d9: 20 ea f7
loc_f7dc:
	rts                      ; $f7dc: 60

; === Block $f7ea-$f7ff (Code) ===
loc_f7ea:
	stx $a0                  ; $f7ea: 86 a0
	lda $ff44,x              ; $f7ec: bd 44 ff
	tax                      ; $f7ef: aa
	lda $04,x                ; $f7f0: b5 04 NUSIZ0
	cmp #$00                 ; $f7f2: c9 00 VSYNC
	bne $f84e                ; $f7f4: d0 58
loc_f7f6:
	lda $0282                ; $f7f6: ad 82 02 SWCHB
	and #$80                 ; $f7f9: 29 80
	beq $f802                ; $f7fb: f0 05
loc_f7fd:
	lda #$00                 ; $f7fd: a9 00 VSYNC
	jmp $f804                ; $f7ff: 4c 04 f8

; === Block $f802-$f84b (Code) ===
loc_f802:
	lda #$b6                 ; $f802: a9 b6
loc_f804:
	sta $d6                  ; $f804: 85 d6
	stx $d5                  ; $f806: 86 d5
	jsr $f715                ; $f808: 20 15 f7
loc_f80b:
	lda $a0                  ; $f80b: a5 a0
	jsr $f6e9                ; $f80d: 20 e9 f6
loc_f810:
	beq $f832                ; $f810: f0 20
loc_f812:
	lda $0282                ; $f812: ad 82 02 SWCHB
	rol                      ; $f815: 2a
	rol                      ; $f816: 2a
	rol                      ; $f817: 2a
	and #$01                 ; $f818: 29 01 VBLANK
	ora $dd                  ; $f81a: 05 dd
	tay                      ; $f81c: a8
	lda $f89f,y              ; $f81d: b9 9f f8
	sta $04,x                ; $f820: 95 04 NUSIZ0
	lda $e3                  ; $f822: a5 e3
	sta $01,x                ; $f824: 95 01 VBLANK
	lda $e4                  ; $f826: a5 e4
	sta $02,x                ; $f828: 95 02 WSYNC
	lda #$01                 ; $f82a: a9 01 VBLANK
	sta $e0                  ; $f82c: 85 e0
	lda #$10                 ; $f82e: a9 10 RESP0
	sta $df                  ; $f830: 85 df
loc_f832:
	stx $9a                  ; $f832: 86 9a
	ldx $a0                  ; $f834: a6 a0
	jsr $f6fe                ; $f836: 20 fe f6
loc_f839:
	ldx $9a                  ; $f839: a6 9a
	cmp #$51                 ; $f83b: c9 51 RESP1
	bne $f84b                ; $f83d: d0 0c
loc_f83f:
	lda #$01                 ; $f83f: a9 01 VBLANK
	sta $04,x                ; $f841: 95 04 NUSIZ0
	lda #$03                 ; $f843: a9 03 RSYNC
	sta $e0                  ; $f845: 85 e0
	lda #$10                 ; $f847: a9 10 RESP0
	sta $df                  ; $f849: 85 df
loc_f84b:
	jmp $f89e                ; $f84b: 4c 9e f8

; === Block $f84e-$f86e (Code) ===
loc_f84e:
	cmp #$01                 ; $f84e: c9 01 VBLANK
	beq $f89e                ; $f850: f0 4c
loc_f852:
	cmp #$02                 ; $f852: c9 02 WSYNC
	bne $f871                ; $f854: d0 1b
loc_f856:
	lda $00,x                ; $f856: b5 00 VSYNC
	sta $8a                  ; $f858: 85 8a
	sta $e2                  ; $f85a: 85 e2
	lda $01,x                ; $f85c: b5 01 VBLANK
	clc                      ; $f85e: 18
	adc #$03                 ; $f85f: 69 03 RSYNC
	sta $8b                  ; $f861: 85 8b
	sta $e3                  ; $f863: 85 e3
	lda $02,x                ; $f865: b5 02 WSYNC
	sec                      ; $f867: 38
	sbc #$0a                 ; $f868: e9 0a CTRLPF
	sta $8c                  ; $f86a: 85 8c
	sta $e4                  ; $f86c: 85 e4
	jmp $f89e                ; $f86e: 4c 9e f8

; === Block $f871-$f89e (Code) ===
loc_f871:
	inc $04,x                ; $f871: f6 04 NUSIZ0
	lda $04,x                ; $f873: b5 04 NUSIZ0
	cmp #$fc                 ; $f875: c9 fc
	bcc $f89e                ; $f877: 90 25
loc_f879:
	lda $a0                  ; $f879: a5 a0
	jsr $f6e9                ; $f87b: 20 e9 f6
loc_f87e:
	beq $f89e                ; $f87e: f0 1e
loc_f880:
	lda #$02                 ; $f880: a9 02 WSYNC
	sta $04,x                ; $f882: 95 04 NUSIZ0
	lda #$02                 ; $f884: a9 02 WSYNC
	sta $e0                  ; $f886: 85 e0
	lda #$10                 ; $f888: a9 10 RESP0
	sta $df                  ; $f88a: 85 df
	lda #$9b                 ; $f88c: a9 9b
	cmp $01,x                ; $f88e: d5 01 VBLANK
	beq $f896                ; $f890: f0 04
loc_f892:
	bcs $f896                ; $f892: b0 02
loc_f894:
	sta $01,x                ; $f894: 95 01 VBLANK
loc_f896:
	lda #$17                 ; $f896: a9 17 AUDF0
	cmp $02,x                ; $f898: d5 02 WSYNC
	bcc $f89e                ; $f89a: 90 02
loc_f89c:
	sta $02,x                ; $f89c: 95 02 WSYNC

; === Block $f89e-$f89e (Code) ===
loc_f89e:
	rts                      ; $f89e: 60

; === Block $f8a5-$f8c0 (Code) ===
loc_f8a5:
	inc $cf                  ; $f8a5: e6 cf
	lda $cf                  ; $f8a7: a5 cf
	cmp #$08                 ; $f8a9: c9 08 COLUPF
	bne $f8b1                ; $f8ab: d0 04
loc_f8ad:
	lda #$00                 ; $f8ad: a9 00 VSYNC
	sta $cf                  ; $f8af: 85 cf
loc_f8b1:
	lda $d1                  ; $f8b1: a5 d1
	beq $f8c3                ; $f8b3: f0 0e
loc_f8b5:
	inc $d1                  ; $f8b5: e6 d1
	lda $ce                  ; $f8b7: a5 ce
	ldx #$cb                 ; $f8b9: a2 cb
	ldy #$03                 ; $f8bb: a0 03 RSYNC
	jsr $f5ff                ; $f8bd: 20 ff f5
loc_f8c0:
	jmp $f908                ; $f8c0: 4c 08 f9

; === Block $f8c3-$f926 (Code) ===
loc_f8c3:
	lda #$cb                 ; $f8c3: a9 cb
	sta $d5                  ; $f8c5: 85 d5
	lda #$03                 ; $f8c7: a9 03 RSYNC
	sta $d4                  ; $f8c9: 85 d4
	lda #$27                 ; $f8cb: a9 27 VDELBL
	sta $d2                  ; $f8cd: 85 d2
	lda #$f9                 ; $f8cf: a9 f9
	sta $d3                  ; $f8d1: 85 d3
	lda $d0                  ; $f8d3: a5 d0
	sta $d6                  ; $f8d5: 85 d6
	jsr $f715                ; $f8d7: 20 15 f7
loc_f8da:
	ldy $e1                  ; $f8da: a4 e1
	lda ($d2),y              ; $f8dc: b1 d2
	beq $f908                ; $f8de: f0 28
loc_f8e0:
	iny                      ; $f8e0: c8
	lda ($d2),y              ; $f8e1: b1 d2
	tax                      ; $f8e3: aa
	lda $00,x                ; $f8e4: b5 00 VSYNC
	cmp $cb                  ; $f8e6: c5 cb
	bne $f908                ; $f8e8: d0 1e
loc_f8ea:
	lda $01,x                ; $f8ea: b5 01 VBLANK
	sec                      ; $f8ec: 38
	sbc $cc                  ; $f8ed: e5 cc
	clc                      ; $f8ef: 18
	adc #$04                 ; $f8f0: 69 04 NUSIZ0
	and #$f8                 ; $f8f2: 29 f8
	bne $f908                ; $f8f4: d0 12
loc_f8f6:
	lda $02,x                ; $f8f6: b5 02 WSYNC
	sec                      ; $f8f8: 38
	sbc $cd                  ; $f8f9: e5 cd
	clc                      ; $f8fb: 18
	adc #$04                 ; $f8fc: 69 04 NUSIZ0
	and #$f8                 ; $f8fe: 29 f8
	bne $f908                ; $f900: d0 06
loc_f902:
	stx $d0                  ; $f902: 86 d0
	lda #$10                 ; $f904: a9 10 RESP0
	sta $d1                  ; $f906: 85 d1
loc_f908:
	ldx $d0                  ; $f908: a6 d0
	lda $cb                  ; $f90a: a5 cb
	sta $00,x                ; $f90c: 95 00 VSYNC
	lda $cc                  ; $f90e: a5 cc
	clc                      ; $f910: 18
	adc #$08                 ; $f911: 69 08 COLUPF
	sta $01,x                ; $f913: 95 01 VBLANK
	lda $cd                  ; $f915: a5 cd
	sta $02,x                ; $f917: 95 02 WSYNC
	lda $d0                  ; $f919: a5 d0
	ldy $9d                  ; $f91b: a4 9d
	cmp $ff44,y              ; $f91d: d9 44 ff
	bne $f926                ; $f920: d0 04
loc_f922:
	lda #$a2                 ; $f922: a9 a2
	sta $9d                  ; $f924: 85 9d
loc_f926:
	rts                      ; $f926: 60

; === Block $f93c-$f965 (Code) ===
loc_f93c:
	ldy #$02                 ; $f93c: a0 02 WSYNC
	ldx $f9a7,y              ; $f93e: be a7 f9
	jsr $f6fe                ; $f941: 20 fe f6
loc_f944:
	sta $97                  ; $f944: 85 97
	cmp $f9aa,y              ; $f946: d9 aa f9
	bne $f94f                ; $f949: d0 04
loc_f94b:
	tya                      ; $f94b: 98
	tax                      ; $f94c: aa
	inc $c8,x                ; $f94d: f6 c8
loc_f94f:
	tya                      ; $f94f: 98
	tax                      ; $f950: aa
	lda $c8,x                ; $f951: b5 c8
	cmp #$1c                 ; $f953: c9 1c GRP1
	beq $f988                ; $f955: f0 31
loc_f957:
	lda $f9a7,y              ; $f957: b9 a7 f9
	jsr $f6e9                ; $f95a: 20 e9 f6
loc_f95d:
	beq $f968                ; $f95d: f0 09
loc_f95f:
	lda #$01                 ; $f95f: a9 01 VBLANK
	sta $c8,x                ; $f961: 95 c8
	ldx #$8a                 ; $f963: a2 8a
	jmp $f97f                ; $f965: 4c 7f f9

; === Block $f968-$f979 (Code) ===
loc_f968:
	lda $97                  ; $f968: a5 97
	cmp #$a2                 ; $f96a: c9 a2
	beq $f97c                ; $f96c: f0 0e
loc_f96e:
	ldx $97                  ; $f96e: a6 97
	sty $9a                  ; $f970: 84 9a
	jsr $f2e4                ; $f972: 20 e4 f2
loc_f975:
	ldy $9a                  ; $f975: a4 9a
	ldx $93                  ; $f977: a6 93
	jmp $f97f                ; $f979: 4c 7f f9

; === Block $f97c-$f97c (Code) ===
loc_f97c:
	jmp $f988                ; $f97c: 4c 88 f9

; === Block $f97f-$f988 (Code) ===
loc_f97f:
	lda $f9ad,y              ; $f97f: b9 ad f9
	sta $00,x                ; $f982: 95 00 VSYNC
	lda #$10                 ; $f984: a9 10 RESP0
	sta $02,x                ; $f986: 95 02 WSYNC

; === Block $f988-$f9a3 (Code) ===
loc_f988:
	tya                      ; $f988: 98
	tax                      ; $f989: aa
	lda $c8,x                ; $f98a: b5 c8
	cmp #$01                 ; $f98c: c9 01 VBLANK
	beq $f9a0                ; $f98e: f0 10
loc_f990:
	cmp #$1c                 ; $f990: c9 1c GRP1
	beq $f9a0                ; $f992: f0 0c
loc_f994:
	inc $c8,x                ; $f994: f6 c8
	lda $c8,x                ; $f996: b5 c8
	cmp #$38                 ; $f998: c9 38
	bne $f9a0                ; $f99a: d0 04
loc_f99c:
	lda #$01                 ; $f99c: a9 01 VBLANK
	sta $c8,x                ; $f99e: 95 c8
loc_f9a0:
	dey                      ; $f9a0: 88
	bmi $f9a6                ; $f9a1: 30 03
loc_f9a3:
	jmp $f93e                ; $f9a3: 4c 3e f9

; === Block $f9a6-$f9a6 (Code) ===
loc_f9a6:
	rts                      ; $f9a6: 60

; === Block $f9b3-$f9d9 (Code) ===
loc_f9b3:
	lda $b5                  ; $f9b3: a5 b5
	sec                      ; $f9b5: 38
	sbc #$08                 ; $f9b6: e9 08 COLUPF
	sta $b5                  ; $f9b8: 85 b5
	lda #$00                 ; $f9ba: a9 00 VSYNC
	sta $d6                  ; $f9bc: 85 d6
	lda #$da                 ; $f9be: a9 da
	sta $d2                  ; $f9c0: 85 d2
	lda #$f9                 ; $f9c2: a9 f9
	sta $d3                  ; $f9c4: 85 d3
	jsr $f728                ; $f9c6: 20 28 f7
loc_f9c9:
	lda $9b                  ; $f9c9: a5 9b
	beq $f9d2                ; $f9cb: f0 05
loc_f9cd:
	ldy #$01                 ; $f9cd: a0 01 VBLANK
	jsr $f5ff                ; $f9cf: 20 ff f5
loc_f9d2:
	lda $b5                  ; $f9d2: a5 b5
	clc                      ; $f9d4: 18
	adc #$08                 ; $f9d5: 69 08 COLUPF
	sta $b5                  ; $f9d7: 85 b5
	rts                      ; $f9d9: 60

; === Block $f9e7-$f9f8 (Code) ===
loc_f9e7:
	lda $8a                  ; $f9e7: a5 8a
	jsr $f271                ; $f9e9: 20 71 f2
loc_f9ec:
	ldy #$02                 ; $f9ec: a0 02 WSYNC
	lda ($93),y              ; $f9ee: b1 93
	cmp #$08                 ; $f9f0: c9 08 COLUPF
	beq $f9fb                ; $f9f2: f0 07
loc_f9f4:
	lda #$00                 ; $f9f4: a9 00 VSYNC
	sta $db                  ; $f9f6: 85 db
	jmp $fa22                ; $f9f8: 4c 22 fa

; === Block $f9fb-$fa17 (Code) ===
loc_f9fb:
	lda $8a                  ; $f9fb: a5 8a
	sta $d9                  ; $f9fd: 85 d9
	lda $8b                  ; $f9ff: a5 8b
	sec                      ; $fa01: 38
	sbc #$0e                 ; $fa02: e9 0e PF1
	sta $da                  ; $fa04: 85 da
	lda $8c                  ; $fa06: a5 8c
	clc                      ; $fa08: 18
	adc #$0e                 ; $fa09: 69 0e PF1
	sta $db                  ; $fa0b: 85 db
	lda $da                  ; $fa0d: a5 da
	cmp #$f0                 ; $fa0f: c9 f0
	bcc $fa1a                ; $fa11: 90 07
loc_fa13:
	lda #$01                 ; $fa13: a9 01 VBLANK
	sta $da                  ; $fa15: 85 da
	jmp $fa22                ; $fa17: 4c 22 fa

; === Block $fa1a-$fa22 (Code) ===
loc_fa1a:
	cmp #$82                 ; $fa1a: c9 82
	bcc $fa22                ; $fa1c: 90 04
loc_fa1e:
	lda #$81                 ; $fa1e: a9 81
	sta $da                  ; $fa20: 85 da

; === Block $fa22-$fa22 (Code) ===
loc_fa22:
	rts                      ; $fa22: 60

; === Block $fa23-$fa2b (Code) ===
loc_fa23:
	lda $df                  ; $fa23: a5 df
	bne $fa2c                ; $fa25: d0 05
loc_fa27:
	sta $19                  ; $fa27: 85 19 AUDV0
	sta $1a                  ; $fa29: 85 1a AUDV1
	rts                      ; $fa2b: 60

; === Block $fa2c-$fa46 (Code) ===
loc_fa2c:
	dec $df                  ; $fa2c: c6 df
	lda $e0                  ; $fa2e: a5 e0
	beq $fa47                ; $fa30: f0 15
loc_fa32:
	cmp #$01                 ; $fa32: c9 01 VBLANK
	beq $fa55                ; $fa34: f0 1f
loc_fa36:
	cmp #$02                 ; $fa36: c9 02 WSYNC
	beq $fa6c                ; $fa38: f0 32
loc_fa3a:
	cmp #$03                 ; $fa3a: c9 03 RSYNC
	beq $fa7f                ; $fa3c: f0 41
loc_fa3e:
	cmp #$04                 ; $fa3e: c9 04 NUSIZ0
	beq $fa8c                ; $fa40: f0 4a
loc_fa42:
	cmp #$05                 ; $fa42: c9 05 NUSIZ1
	beq $fa9b                ; $fa44: f0 55
loc_fa46:
	rts                      ; $fa46: 60

; === Block $fa47-$fa54 (Code) ===
loc_fa47:
	lda $df                  ; $fa47: a5 df
	sta $08                  ; $fa49: 85 08 COLUPF
	sta $15                  ; $fa4b: 85 15 AUDC0
	lsr                      ; $fa4d: 4a
	sta $19                  ; $fa4e: 85 19 AUDV0
	lsr                      ; $fa50: 4a
	lsr                      ; $fa51: 4a
	sta $17                  ; $fa52: 85 17 AUDF0
	rts                      ; $fa54: 60

; === Block $fa55-$fa6b (Code) ===
loc_fa55:
	lda $df                  ; $fa55: a5 df
	lsr                      ; $fa57: 4a
	lda #$03                 ; $fa58: a9 03 RSYNC
	bcs $fa5e                ; $fa5a: b0 02
loc_fa5c:
	lda #$08                 ; $fa5c: a9 08 COLUPF
loc_fa5e:
	sta $15                  ; $fa5e: 85 15 AUDC0
	lda $df                  ; $fa60: a5 df
	sta $19                  ; $fa62: 85 19 AUDV0
	lsr                      ; $fa64: 4a
	lsr                      ; $fa65: 4a
	clc                      ; $fa66: 18
	adc #$1c                 ; $fa67: 69 1c GRP1
	sta $17                  ; $fa69: 85 17 AUDF0
	rts                      ; $fa6b: 60

; === Block $fa6c-$fa7e (Code) ===
loc_fa6c:
	lda #$06                 ; $fa6c: a9 06 COLUP0
	sta $15                  ; $fa6e: 85 15 AUDC0
	lda $df                  ; $fa70: a5 df
	eor #$0f                 ; $fa72: 49 0f PF2
	sta $17                  ; $fa74: 85 17 AUDF0
	lda $df                  ; $fa76: a5 df
	lsr                      ; $fa78: 4a
	clc                      ; $fa79: 18
	adc #$08                 ; $fa7a: 69 08 COLUPF
	sta $19                  ; $fa7c: 85 19 AUDV0
	rts                      ; $fa7e: 60

; === Block $fa7f-$fa8b (Code) ===
loc_fa7f:
	lda #$04                 ; $fa7f: a9 04 NUSIZ0
	sta $15                  ; $fa81: 85 15 AUDC0
	lda $df                  ; $fa83: a5 df
	sta $19                  ; $fa85: 85 19 AUDV0
	eor #$1f                 ; $fa87: 49 1f ENABL
	sta $17                  ; $fa89: 85 17 AUDF0
	rts                      ; $fa8b: 60

; === Block $fa8c-$fa9a (Code) ===
loc_fa8c:
	lda $df                  ; $fa8c: a5 df
	eor #$03                 ; $fa8e: 49 03 RSYNC
	sta $17                  ; $fa90: 85 17 AUDF0
	lda #$05                 ; $fa92: a9 05 NUSIZ1
	sta $19                  ; $fa94: 85 19 AUDV0
	lda #$06                 ; $fa96: a9 06 COLUP0
	sta $15                  ; $fa98: 85 15 AUDC0
	rts                      ; $fa9a: 60

; === Block $fa9b-$fa9d (Code) ===
loc_fa9b:
	lda $df                  ; $fa9b: a5 df
	jmp $fa90                ; $fa9d: 4c 90 fa

