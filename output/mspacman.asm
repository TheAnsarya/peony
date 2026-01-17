; 🌺 Peony Disassembly
; ROM: Ms. Pac-Man (1982) (Atari) (PAL) [!].a26
; Platform: Atari 2600
; Size: 8192 bytes
; Mapper: F8

; === Block $f000-$f020 (Code) ===
reset:
	nop                      ; $f000: ea
	nop                      ; $f001: ea
	nop                      ; $f002: ea
	sei                      ; $f003: 78
	cld                      ; $f004: d8
	ldx #$ff                 ; $f005: a2 ff
	txs                      ; $f007: 9a
	lda #$00                 ; $f008: a9 00 VSYNC
	sta $02,x                ; $f00a: 95 02 WSYNC
	dex                      ; $f00c: ca
	bne $f00a                ; $f00d: d0 fb
loc_f00f:
	lda #$21                 ; $f00f: a9 21 HMP1
	sta $0a                  ; $f011: 85 0a CTRLPF
	lda #$03                 ; $f013: a9 03 RSYNC
	sta $93                  ; $f015: 85 93
	lda #$40                 ; $f017: a9 40 VSYNC
	sta $94                  ; $f019: 85 94
	jsr $f025                ; $f01b: 20 25 f0
loc_f01e:
	dec $fa                  ; $f01e: c6 fa
	jmp $f092                ; $f020: 4c 92 f0

; === Block $f023-$f025 (Code) ===
loc_f023:
	stx $fb                  ; $f023: 86 fb

; === Block $f025-$f091 (Code) ===
loc_f025:
	lda #$00                 ; $f025: a9 00 VSYNC
	sta $f8                  ; $f027: 85 f8
	sta $f9                  ; $f029: 85 f9
	sta $fa                  ; $f02b: 85 fa
	sta $f6                  ; $f02d: 85 f6
	inc $fb                  ; $f02f: e6 fb
	bit $f6                  ; $f031: 24 f6
	bvs $f040                ; $f033: 70 0b
loc_f035:
	lda $fb                  ; $f035: a5 fb
	lsr                      ; $f037: 4a
	lsr                      ; $f038: 4a
	lsr                      ; $f039: 4a
	lsr                      ; $f03a: 4a
	lsr                      ; $f03b: 4a
	and #$03                 ; $f03c: 29 03 RSYNC
	sta $80                  ; $f03e: 85 80
loc_f040:
	jsr $fd04                ; $f040: 20 04 fd
loc_f043:
	lda #$00                 ; $f043: a9 00 VSYNC
	sta $a7                  ; $f045: 85 a7
	sta $9f                  ; $f047: 85 9f
	sta $f7                  ; $f049: 85 f7
	lda #$3c                 ; $f04b: a9 3c
	sta $f5                  ; $f04d: 85 f5
	lda #$58                 ; $f04f: a9 58 AUDF1
	sta $86                  ; $f051: 85 86
	sta $87                  ; $f053: 85 87
	sta $88                  ; $f055: 85 88
	sta $8a                  ; $f057: 85 8a
	sta $89                  ; $f059: 85 89
	lda #$50                 ; $f05b: a9 50 RESP0
	sta $8c                  ; $f05d: 85 8c
	sta $8d                  ; $f05f: 85 8d
	sta $8e                  ; $f061: 85 8e
	lda #$32                 ; $f063: a9 32
	sta $8f                  ; $f065: 85 8f
	lda #$62                 ; $f067: a9 62 HMM0
	sta $90                  ; $f069: 85 90
	lda #$03                 ; $f06b: a9 03 RSYNC
	sta $85                  ; $f06d: 85 85
	lda #$70                 ; $f06f: a9 70
	sta $81                  ; $f071: 85 81
	lda #$72                 ; $f073: a9 72
	sta $82                  ; $f075: 85 82
	lda #$73                 ; $f077: a9 73
	sta $83                  ; $f079: 85 83
	lda $9e                  ; $f07b: a5 9e
	and #$30                 ; $f07d: 29 30
	sta $84                  ; $f07f: 85 84
	lda #$00                 ; $f081: a9 00 VSYNC
	sta $92                  ; $f083: 85 92
	sta $f4                  ; $f085: 85 f4
	sta $8b                  ; $f087: 85 8b
	lda $f6                  ; $f089: a5 f6
	and #$40                 ; $f08b: 29 40 VSYNC
	sta $f6                  ; $f08d: 85 f6
	dec $fb                  ; $f08f: c6 fb
	rts                      ; $f091: 60

; === Block $f092-$f12a (Code) ===
loc_f092:
	lda #$02                 ; $f092: a9 02 WSYNC
	sta $02                  ; $f094: 85 02 WSYNC
	sta $00                  ; $f096: 85 00 VSYNC
	sta $01                  ; $f098: 85 01 VBLANK
	sta $02                  ; $f09a: 85 02 WSYNC
	lda $a7                  ; $f09c: a5 a7
	cmp #$ff                 ; $f09e: c9 ff
	bne $f0cf                ; $f0a0: d0 2d
loc_f0a2:
	bit $85                  ; $f0a2: 24 85
	bpl $f0c9                ; $f0a4: 10 23
loc_f0a6:
	lda $85                  ; $f0a6: a5 85
	tay                      ; $f0a8: a8
	and #$30                 ; $f0a9: 29 30
	tax                      ; $f0ab: aa
	tya                      ; $f0ac: 98
	and #$cf                 ; $f0ad: 29 cf
	sta $85                  ; $f0af: 85 85
	txa                      ; $f0b1: 8a
	adc #$0f                 ; $f0b2: 69 0f PF2
	and #$30                 ; $f0b4: 29 30
	ora $85                  ; $f0b6: 05 85
	sta $85                  ; $f0b8: 85 85
	txa                      ; $f0ba: 8a
	lsr                      ; $f0bb: 4a
	lsr                      ; $f0bc: 4a
	lsr                      ; $f0bd: 4a
	lsr                      ; $f0be: 4a
	tax                      ; $f0bf: aa
	ldy $f319,x              ; $f0c0: bc 19 f3
	lda #$0f                 ; $f0c3: a9 0f PF2
	ldx #$05                 ; $f0c5: a2 05 NUSIZ1
	bne $f0cb                ; $f0c7: d0 02
loc_f0c9:
	lda #$00                 ; $f0c9: a9 00 VSYNC
loc_f0cb:
	stx $15                  ; $f0cb: 86 15 AUDC0
	sty $17                  ; $f0cd: 84 17 AUDF0
loc_f0cf:
	sta $02                  ; $f0cf: 85 02 WSYNC
	ldx $a7                  ; $f0d1: a6 a7
	cpx #$ff                 ; $f0d3: e0 ff
	bne $f0dd                ; $f0d5: d0 06
loc_f0d7:
	bit $94                  ; $f0d7: 24 94
	bvs $f0dd                ; $f0d9: 70 02
loc_f0db:
	sta $19                  ; $f0db: 85 19 AUDV0
loc_f0dd:
	sta $02                  ; $f0dd: 85 02 WSYNC
	lda #$00                 ; $f0df: a9 00 VSYNC
	sta $00                  ; $f0e1: 85 00 VSYNC
	lda #$4d                 ; $f0e3: a9 4d PF0
	sta $0296                ; $f0e5: 8d 96 02 TIM64T
	lda #$b0                 ; $f0e8: a9 b0
	bit $94                  ; $f0ea: 24 94
	bpl $f0f0                ; $f0ec: 10 02
loc_f0ee:
	lda #$00                 ; $f0ee: a9 00 VSYNC
loc_f0f0:
	sta $09                  ; $f0f0: 85 09 COLUBK
	lda $a7                  ; $f0f2: a5 a7
	cmp #$53                 ; $f0f4: c9 53 RESM1
	bne $f134                ; $f0f6: d0 3c
loc_f0f8:
	lda $fb                  ; $f0f8: a5 fb
	and #$0f                 ; $f0fa: 29 0f PF2
	bne $f12d                ; $f0fc: d0 2f
loc_f0fe:
	ldx $93                  ; $f0fe: a6 93
	stx $f7                  ; $f100: 86 f7
	ldx $94                  ; $f102: a6 94
	stx $91                  ; $f104: 86 91
	sta $f6                  ; $f106: 85 f6
	sta $19                  ; $f108: 85 19 AUDV0
	sta $1a                  ; $f10a: 85 1a AUDV1
	sta $fb                  ; $f10c: 85 fb
	sta $80                  ; $f10e: 85 80
	tax                      ; $f110: aa
	lda #$c0                 ; $f111: a9 c0
	sta $94                  ; $f113: 85 94
	lda #$98                 ; $f115: a9 98
	sta $8c                  ; $f117: 85 8c
	stx $9f                  ; $f119: 86 9f
	stx $93                  ; $f11b: 86 93
	dex                      ; $f11d: ca
	stx $a7                  ; $f11e: 86 a7
	lda #$aa                 ; $f120: a9 aa
	sta $90                  ; $f122: 85 90
	lda #$a2                 ; $f124: a9 a2
	sta $86                  ; $f126: 85 86
	inc $98                  ; $f128: e6 98
	jmp $f134                ; $f12a: 4c 34 f1

; === Block $f12d-$f134 (Code) ===
loc_f12d:
	lda #$23                 ; $f12d: a9 23 HMM1
	sta $a7                  ; $f12f: 85 a7
	jsr $f04f                ; $f131: 20 4f f0

; === Block $f134-$f1a2 (Code) ===
loc_f134:
	lda $0282                ; $f134: ad 82 02 SWCHB
	lsr                      ; $f137: 4a
	bcc $f179                ; $f138: 90 3f
loc_f13a:
	lsr                      ; $f13a: 4a
	lda $95                  ; $f13b: a5 95
	beq $f143                ; $f13d: f0 04
loc_f13f:
	dec $95                  ; $f13f: c6 95
	bpl $f171                ; $f141: 10 2e
loc_f143:
	bcs $f171                ; $f143: b0 2c
loc_f145:
	sta $19                  ; $f145: 85 19 AUDV0
	sta $1a                  ; $f147: 85 1a AUDV1
	sta $f8                  ; $f149: 85 f8
	sta $f9                  ; $f14b: 85 f9
	sta $fa                  ; $f14d: 85 fa
	bit $94                  ; $f14f: 24 94
	bpl $f157                ; $f151: 10 04
loc_f153:
	lda $f7                  ; $f153: a5 f7
	sta $93                  ; $f155: 85 93
loc_f157:
	inc $93                  ; $f157: e6 93
	lda $93                  ; $f159: a5 93
	and #$03                 ; $f15b: 29 03 RSYNC
	sta $93                  ; $f15d: 85 93
	ldx #$0f                 ; $f15f: a2 0f PF2
	stx $95                  ; $f161: 86 95
	bit $94                  ; $f163: 24 94
	bmi $f169                ; $f165: 30 02
loc_f167:
	bvs $f190                ; $f167: 70 27
loc_f169:
	lda $94                  ; $f169: a5 94
	and #$6f                 ; $f16b: 29 6f
	ora #$40                 ; $f16d: 09 40 VSYNC
	bne $f189                ; $f16f: d0 18
loc_f171:
	bit $94                  ; $f171: 24 94
	bvc $f190                ; $f173: 50 1b
loc_f175:
	bit $3c                  ; $f175: 24 3c
	bmi $f190                ; $f177: 30 17
loc_f179:
	bit $94                  ; $f179: 24 94
	bpl $f181                ; $f17b: 10 04
loc_f17d:
	lda $f7                  ; $f17d: a5 f7
	sta $93                  ; $f17f: 85 93
loc_f181:
	lda $94                  ; $f181: a5 94
	and #$0f                 ; $f183: 29 0f PF2
	ldx #$02                 ; $f185: a2 02 WSYNC
	bne $f18b                ; $f187: d0 02
loc_f189:
	ldx #$00                 ; $f189: a2 00 VSYNC
loc_f18b:
	sta $94                  ; $f18b: 85 94
	jsr $f023                ; $f18d: 20 23 f0
loc_f190:
	inc $9e                  ; $f190: e6 9e
	lda #$01                 ; $f192: a9 01 VBLANK
	sta $96                  ; $f194: 85 96
	bit $94                  ; $f196: 24 94
	bpl $f1a5                ; $f198: 10 0b
loc_f19a:
	ldy #$04                 ; $f19a: a0 04 NUSIZ0
	jsr $f345                ; $f19c: 20 45 f3
loc_f19f:
	dey                      ; $f19f: 88
	bpl $f19c                ; $f1a0: 10 fa
loc_f1a2:
	jmp $fff2                ; $f1a2: 4c f2 ff

; === Block $f1a5-$f1d2 (Code) ===
loc_f1a5:
	jsr $f44d                ; $f1a5: 20 4d f4
loc_f1a8:
	lda $a7                  ; $f1a8: a5 a7
	cmp #$ff                 ; $f1aa: c9 ff
	beq $f1e6                ; $f1ac: f0 38
loc_f1ae:
	cmp #$39                 ; $f1ae: c9 39
	beq $f1df                ; $f1b0: f0 2d
loc_f1b2:
	cmp #$3d                 ; $f1b2: c9 3d
	beq $f1df                ; $f1b4: f0 29
loc_f1b6:
	cmp #$41                 ; $f1b6: c9 41 VBLANK
	beq $f1df                ; $f1b8: f0 25
loc_f1ba:
	cmp #$45                 ; $f1ba: c9 45 NUSIZ1
	beq $f1df                ; $f1bc: f0 21
loc_f1be:
	cmp #$3b                 ; $f1be: c9 3b
	beq $f1d5                ; $f1c0: f0 13
loc_f1c2:
	cmp #$3f                 ; $f1c2: c9 3f
	beq $f1d5                ; $f1c4: f0 0f
loc_f1c6:
	cmp #$43                 ; $f1c6: c9 43 RSYNC
	beq $f1d5                ; $f1c8: f0 0b
loc_f1ca:
	cmp #$47                 ; $f1ca: c9 47 COLUP1
	beq $f1d5                ; $f1cc: f0 07
loc_f1ce:
	cmp #$48                 ; $f1ce: c9 48 COLUPF
	bne $f1e3                ; $f1d0: d0 11
loc_f1d2:
	jmp $f2b7                ; $f1d2: 4c b7 f2

; === Block $f1d5-$f1dc (Code) ===
loc_f1d5:
	ldy $80                  ; $f1d5: a4 80
	lda $fd16,y              ; $f1d7: b9 16 fd
	sta $08                  ; $f1da: 85 08 COLUPF
	jmp $f2e9                ; $f1dc: 4c e9 f2

; === Block $f1df-$f1e3 (Code) ===
loc_f1df:
	lda #$0d                 ; $f1df: a9 0d PF0
	sta $08                  ; $f1e1: 85 08 COLUPF
loc_f1e3:
	jmp $f2e9                ; $f1e3: 4c e9 f2

; === Block $f1e6-$f1eb (Code) ===
loc_f1e6:
	lda $9e                  ; $f1e6: a5 9e
	lsr                      ; $f1e8: 4a
	bcs $f1ee                ; $f1e9: b0 03
loc_f1eb:
	jmp $f261                ; $f1eb: 4c 61 f2

; === Block $f1ee-$f289 (Code) ===
loc_f1ee:
	ldy #$03                 ; $f1ee: a0 03 RSYNC
	sty $b8                  ; $f1f0: 84 b8
	ldy $b8                  ; $f1f2: a4 b8
	lda $fb                  ; $f1f4: a5 fb
	and #$f0                 ; $f1f6: 29 f0
	bne $f202                ; $f1f8: d0 08
loc_f1fa:
	lda $9e                  ; $f1fa: a5 9e
	and #$1e                 ; $f1fc: 29 1e ENAM1
	cmp #$1e                 ; $f1fe: c9 1e ENAM1
	beq $f25b                ; $f200: f0 59
loc_f202:
	lda $0081,y              ; $f202: b9 81 00
	bpl $f210                ; $f205: 10 09
loc_f207:
	lsr                      ; $f207: 4a
	bcc $f210                ; $f208: 90 06
loc_f20a:
	lda $9e                  ; $f20a: a5 9e
	and #$06                 ; $f20c: 29 06 COLUP0
	beq $f25b                ; $f20e: f0 4b
loc_f210:
	jsr $f9ce                ; $f210: 20 ce f9
loc_f213:
	lda $0081,y              ; $f213: b9 81 00
	bmi $f25b                ; $f216: 30 43
loc_f218:
	and #$70                 ; $f218: 29 70
	cmp #$40                 ; $f21a: c9 40 VSYNC
	beq $f258                ; $f21c: f0 3a
loc_f21e:
	lda $0081,y              ; $f21e: b9 81 00
	lsr                      ; $f221: 4a
	bcs $f22c                ; $f222: b0 08
loc_f224:
	lda $9e                  ; $f224: a5 9e
	adc $b8                  ; $f226: 65 b8
	and #$02                 ; $f228: 29 02 WSYNC
	bne $f258                ; $f22a: d0 2c
loc_f22c:
	cpy #$00                 ; $f22c: c0 00 VSYNC
	beq $f25b                ; $f22e: f0 2b
loc_f230:
	lda $9e                  ; $f230: a5 9e
	and #$0e                 ; $f232: 29 0e PF1
	cpy #$01                 ; $f234: c0 01 VBLANK
	beq $f254                ; $f236: f0 1c
loc_f238:
	cpy #$02                 ; $f238: c0 02 WSYNC
	beq $f24a                ; $f23a: f0 0e
loc_f23c:
	cmp #$00                 ; $f23c: c9 00 VSYNC
	beq $f258                ; $f23e: f0 18
loc_f240:
	cmp #$08                 ; $f240: c9 08 COLUPF
	beq $f258                ; $f242: f0 14
loc_f244:
	cmp #$0c                 ; $f244: c9 0c REFP1
	beq $f258                ; $f246: f0 10
loc_f248:
	bne $f25b                ; $f248: d0 11
loc_f24a:
	cmp #$02                 ; $f24a: c9 02 WSYNC
	beq $f258                ; $f24c: f0 0a
loc_f24e:
	cmp #$0a                 ; $f24e: c9 0a CTRLPF
	beq $f258                ; $f250: f0 06
loc_f252:
	bne $f25b                ; $f252: d0 07
loc_f254:
	cmp #$06                 ; $f254: c9 06 COLUP0
	bne $f25b                ; $f256: d0 03
loc_f258:
	jsr $f9ce                ; $f258: 20 ce f9
loc_f25b:
	dec $b8                  ; $f25b: c6 b8
	bpl $f1f2                ; $f25d: 10 93
loc_f25f:
	bmi $f2a8                ; $f25f: 30 47
loc_f261:
	ldy #$04                 ; $f261: a0 04 NUSIZ0
	sty $b8                  ; $f263: 84 b8
	jsr $f9ce                ; $f265: 20 ce f9
loc_f268:
	lda $85                  ; $f268: a5 85
	lsr                      ; $f26a: 4a
	bcs $f273                ; $f26b: b0 06
loc_f26d:
	lda $9e                  ; $f26d: a5 9e
	and #$02                 ; $f26f: 29 02 WSYNC
	bne $f28c                ; $f271: d0 19
loc_f273:
	lda $9e                  ; $f273: a5 9e
	and #$02                 ; $f275: 29 02 WSYNC
	bne $f28f                ; $f277: d0 16
loc_f279:
	bit $85                  ; $f279: 24 85
	bpl $f28c                ; $f27b: 10 0f
loc_f27d:
	bvc $f283                ; $f27d: 50 04
loc_f27f:
	lda #$bf                 ; $f27f: a9 bf
	bmi $f285                ; $f281: 30 02
loc_f283:
	lda #$3f                 ; $f283: a9 3f
loc_f285:
	and $85                  ; $f285: 25 85
	sta $85                  ; $f287: 85 85
	jmp $f28f                ; $f289: 4c 8f f2

; === Block $f28c-$f29f (Code) ===
loc_f28c:
	jsr $f9ce                ; $f28c: 20 ce f9
loc_f28f:
	lda $9e                  ; $f28f: a5 9e
	and #$02                 ; $f291: 29 02 WSYNC
	bne $f298                ; $f293: d0 03
loc_f295:
	jsr $f657                ; $f295: 20 57 f6
loc_f298:
	bit $32                  ; $f298: 24 32
	bvc $f2a2                ; $f29a: 50 06
loc_f29c:
	jsr $f52f                ; $f29c: 20 2f f5
loc_f29f:
	jmp $f2a5                ; $f29f: 4c a5 f2

; === Block $f2a2-$f2a8 (Code) ===
loc_f2a2:
	jsr $f5b1                ; $f2a2: 20 b1 f5
loc_f2a5:
	jsr $f3e9                ; $f2a5: 20 e9 f3

; === Block $f2a8-$f2b7 (Code) ===
loc_f2a8:
	lda $f7                  ; $f2a8: a5 f7
	ldx $80                  ; $f2aa: a6 80
	cmp $f315,x              ; $f2ac: dd 15 f3
	bne $f2e9                ; $f2af: d0 38
loc_f2b1:
	lda #$37                 ; $f2b1: a9 37
	sta $a7                  ; $f2b3: 85 a7
	bpl $f2e9                ; $f2b5: 10 32

; === Block $f2b7-$f301 (Code) ===
loc_f2b7:
	bit $f6                  ; $f2b7: 24 f6
	bvs $f2c9                ; $f2b9: 70 0e
loc_f2bb:
	lda $fb                  ; $f2bb: a5 fb
	cmp #$60                 ; $f2bd: c9 60 HMP0
	bmi $f2dd                ; $f2bf: 30 1c
loc_f2c1:
	lda $f6                  ; $f2c1: a5 f6
	ora #$40                 ; $f2c3: 09 40 VSYNC
	sta $f6                  ; $f2c5: 85 f6
	bne $f2cf                ; $f2c7: d0 06
loc_f2c9:
	lda $80                  ; $f2c9: a5 80
	eor #$01                 ; $f2cb: 49 01 VBLANK
	sta $80                  ; $f2cd: 85 80
loc_f2cf:
	lda $9e                  ; $f2cf: a5 9e
	and #$70                 ; $f2d1: 29 70
	eor $fb                  ; $f2d3: 45 fb
	cmp #$70                 ; $f2d5: c9 70
	bmi $f2e0                ; $f2d7: 30 07
loc_f2d9:
	and #$6f                 ; $f2d9: 29 6f
	bpl $f2e0                ; $f2db: 10 03
loc_f2dd:
	clc                      ; $f2dd: 18
	adc #$10                 ; $f2de: 69 10 RESP0
loc_f2e0:
	sta $fb                  ; $f2e0: 85 fb
	jsr $f02f                ; $f2e2: 20 2f f0
loc_f2e5:
	lda #$23                 ; $f2e5: a9 23 HMM1
	sta $a7                  ; $f2e7: 85 a7
loc_f2e9:
	sta $2c                  ; $f2e9: 85 2c CXCLR
	lda $9e                  ; $f2eb: a5 9e
	and #$08                 ; $f2ed: 29 08 COLUPF
	beq $f304                ; $f2ef: f0 13
loc_f2f1:
	lda $f5                  ; $f2f1: a5 f5
	and #$7c                 ; $f2f3: 29 7c
	sta $f5                  ; $f2f5: 85 f5
	and #$30                 ; $f2f7: 29 30
	lsr                      ; $f2f9: 4a
	lsr                      ; $f2fa: 4a
	lsr                      ; $f2fb: 4a
	lsr                      ; $f2fc: 4a
	ora $f5                  ; $f2fd: 05 f5
	sta $f5                  ; $f2ff: 85 f5
	jmp $fff2                ; $f301: 4c f2 ff

; === Block $f304-$f312 (Code) ===
loc_f304:
	lda $f5                  ; $f304: a5 f5
	and #$7c                 ; $f306: 29 7c
	sta $f5                  ; $f308: 85 f5
	and #$0c                 ; $f30a: 29 0c REFP1
	lsr                      ; $f30c: 4a
	lsr                      ; $f30d: 4a
	ora $f5                  ; $f30e: 05 f5
	sta $f5                  ; $f310: 85 f5
	jmp $fff2                ; $f312: 4c f2 ff

; === Block $f31d-$f344 (Code) ===
loc_f31d:
	bit $94                  ; $f31d: 24 94
	bvs $f343                ; $f31f: 70 22
loc_f321:
	sed                      ; $f321: f8
	clc                      ; $f322: 18
	adc $f8                  ; $f323: 65 f8
	sta $f8                  ; $f325: 85 f8
	txa                      ; $f327: 8a
	adc $f9                  ; $f328: 65 f9
	sta $f9                  ; $f32a: 85 f9
	lda #$00                 ; $f32c: a9 00 VSYNC
	adc $fa                  ; $f32e: 65 fa
	sta $fa                  ; $f330: 85 fa
	lda $f8                  ; $f332: a5 f8
	lsr                      ; $f334: 4a
	bcs $f343                ; $f335: b0 0c
loc_f337:
	lda $fa                  ; $f337: a5 fa
	beq $f343                ; $f339: f0 08
loc_f33b:
	lda $f8                  ; $f33b: a5 f8
	ora #$01                 ; $f33d: 09 01 VBLANK
	sta $f8                  ; $f33f: 85 f8
	inc $fb                  ; $f341: e6 fb
loc_f343:
	cld                      ; $f343: d8
	rts                      ; $f344: 60

; === Block $f345-$f364 (Code) ===
loc_f345:
	lda #$00                 ; $f345: a9 00 VSYNC
	sta $08                  ; $f347: 85 08 COLUPF
	lda #$10                 ; $f349: a9 10 RESP0
	bit $94                  ; $f34b: 24 94
	bne $f39c                ; $f34d: d0 4d
loc_f34f:
	cpy $9f                  ; $f34f: c4 9f
	bne $f396                ; $f351: d0 43
loc_f353:
	lda $0086,y              ; $f353: b9 86 00
	cpy #$04                 ; $f356: c0 04 NUSIZ0
	beq $f360                ; $f358: f0 06
loc_f35a:
	cmp #$3f                 ; $f35a: c9 3f
	beq $f367                ; $f35c: f0 09
loc_f35e:
	bne $f364                ; $f35e: d0 04
loc_f360:
	cmp #$58                 ; $f360: c9 58 AUDF1
	beq $f397                ; $f362: f0 33
loc_f364:
	jmp $fbbc                ; $f364: 4c bc fb

; === Block $f367-$f36f (Code) ===
loc_f367:
	lda $008c,y              ; $f367: b9 8c 00
	cmp $f3e5,y              ; $f36a: d9 e5 f3
	beq $f372                ; $f36d: f0 03
loc_f36f:
	jmp $fba3                ; $f36f: 4c a3 fb

; === Block $f372-$f389 (Code) ===
loc_f372:
	ldx $9f                  ; $f372: a6 9f
	inx                      ; $f374: e8
	cpx #$04                 ; $f375: e0 04 NUSIZ0
	beq $f37b                ; $f377: f0 02
loc_f379:
	stx $93                  ; $f379: 86 93
loc_f37b:
	stx $9f                  ; $f37b: 86 9f
	lda #$a2                 ; $f37d: a9 a2
	sta $86,x                ; $f37f: 95 86
	cpx #$04                 ; $f381: e0 04 NUSIZ0
	beq $f38a                ; $f383: f0 05
loc_f385:
	lda #$98                 ; $f385: a9 98
	sta $8c,x                ; $f387: 95 8c
	rts                      ; $f389: 60

; === Block $f38a-$f396 (Code) ===
loc_f38a:
	lda #$7f                 ; $f38a: a9 7f
	sta $90                  ; $f38c: 85 90
	lda $85                  ; $f38e: a5 85
	and #$fc                 ; $f390: 29 fc
	ora #$03                 ; $f392: 09 03 RSYNC
	sta $85                  ; $f394: 85 85

; === Block $f396-$f396 (Code) ===
loc_f396:
	rts                      ; $f396: 60

; === Block $f397-$f39b (Code) ===
loc_f397:
	lda #$d0                 ; $f397: a9 d0
	sta $94                  ; $f399: 85 94
	rts                      ; $f39b: 60

; === Block $f39c-$f3b2 (Code) ===
loc_f39c:
	cpy #$04                 ; $f39c: c0 04 NUSIZ0
	beq $f396                ; $f39e: f0 f6
loc_f3a0:
	lda $0086,y              ; $f3a0: b9 86 00
	cmp #$3f                 ; $f3a3: c9 3f
	beq $f3b5                ; $f3a5: f0 0e
loc_f3a7:
	cmp #$71                 ; $f3a7: c9 71
	beq $f3bf                ; $f3a9: f0 14
loc_f3ab:
	lda $008c,y              ; $f3ab: b9 8c 00
	cmp #$66                 ; $f3ae: c9 66 VDELP1
	beq $f3e2                ; $f3b0: f0 30
loc_f3b2:
	jmp $fbc7                ; $f3b2: 4c c7 fb

; === Block $f3b5-$f3bc (Code) ===
loc_f3b5:
	lda $008c,y              ; $f3b5: b9 8c 00
	cmp #$98                 ; $f3b8: c9 98
	beq $f3b2                ; $f3ba: f0 f6
loc_f3bc:
	jmp $fba7                ; $f3bc: 4c a7 fb

; === Block $f3bf-$f3c6 (Code) ===
loc_f3bf:
	lda $008c,y              ; $f3bf: b9 8c 00
	cmp #$66                 ; $f3c2: c9 66 VDELP1
	beq $f3c9                ; $f3c4: f0 03
loc_f3c6:
	jmp $fba3                ; $f3c6: 4c a3 fb

; === Block $f3c9-$f3df (Code) ===
loc_f3c9:
	ldx $94                  ; $f3c9: a6 94
	inx                      ; $f3cb: e8
	stx $94                  ; $f3cc: 86 94
	txa                      ; $f3ce: 8a
	and #$0f                 ; $f3cf: 29 0f PF2
	bne $f3e2                ; $f3d1: d0 0f
loc_f3d3:
	lda $f7                  ; $f3d3: a5 f7
	sta $93                  ; $f3d5: 85 93
	lda $91                  ; $f3d7: a5 91
	ora #$40                 ; $f3d9: 09 40 VSYNC
	sta $94                  ; $f3db: 85 94
	ldy #$00                 ; $f3dd: a0 00 VSYNC
	jmp $f02f                ; $f3df: 4c 2f f0

; === Block $f3e2-$f3e2 (Code) ===
loc_f3e2:
	jmp $fbbc                ; $f3e2: 4c bc fb

; === Block $f3e9-$f435 (Code) ===
loc_f3e9:
	ldx $93                  ; $f3e9: a6 93
	lda $8a                  ; $f3eb: a5 8a
	sec                      ; $f3ed: 38
	sbc $86,x                ; $f3ee: f5 86
	bpl $f3f7                ; $f3f0: 10 05
loc_f3f2:
	eor #$ff                 ; $f3f2: 49 ff
	clc                      ; $f3f4: 18
	adc #$01                 ; $f3f5: 69 01 VBLANK
loc_f3f7:
	sta $b7                  ; $f3f7: 85 b7
	lda $90                  ; $f3f9: a5 90
	sec                      ; $f3fb: 38
	sbc $8c,x                ; $f3fc: f5 8c
	bpl $f405                ; $f3fe: 10 05
loc_f400:
	eor #$ff                 ; $f400: 49 ff
	clc                      ; $f402: 18
	adc #$01                 ; $f403: 69 01 VBLANK
loc_f405:
	clc                      ; $f405: 18
	adc $b7                  ; $f406: 65 b7
	bcs $f449                ; $f408: b0 3f
loc_f40a:
	cmp #$05                 ; $f40a: c9 05 NUSIZ1
	bcs $f449                ; $f40c: b0 3b
loc_f40e:
	lda $81,x                ; $f40e: b5 81
	bpl $f43a                ; $f410: 10 28
loc_f412:
	and #$03                 ; $f412: 29 03 RSYNC
	ora #$40                 ; $f414: 09 40 VSYNC
	sta $81,x                ; $f416: 95 81
	lda #$40                 ; $f418: a9 40 VSYNC
	clc                      ; $f41a: 18
	adc $f4                  ; $f41b: 65 f4
	bcc $f421                ; $f41d: 90 02
loc_f41f:
	lda #$00                 ; $f41f: a9 00 VSYNC
loc_f421:
	sta $f4                  ; $f421: 85 f4
	and #$c0                 ; $f423: 29 c0
	asl                      ; $f425: 0a
	rol                      ; $f426: 2a
	rol                      ; $f427: 2a
	tay                      ; $f428: a8
	ldx $f436,y              ; $f429: be 36 f4
	lda #$00                 ; $f42c: a9 00 VSYNC
	jsr $f31d                ; $f42e: 20 1d f3
loc_f431:
	lda #$33                 ; $f431: a9 33
	sta $a7                  ; $f433: 85 a7
	rts                      ; $f435: 60

; === Block $f43a-$f448 (Code) ===
loc_f43a:
	and #$f0                 ; $f43a: 29 f0
	cmp #$40                 ; $f43c: c9 40 VSYNC
	beq $f449                ; $f43e: f0 09
loc_f440:
	cmp #$60                 ; $f440: c9 60 HMP0
	beq $f449                ; $f442: f0 05
loc_f444:
	lda #$49                 ; $f444: a9 49 COLUBK
	sta $a7                  ; $f446: 85 a7
	rts                      ; $f448: 60

; === Block $f449-$f44c (Code) ===
loc_f449:
	dex                      ; $f449: ca
	bpl $f3eb                ; $f44a: 10 9f
loc_f44c:
	rts                      ; $f44c: 60

; === Block $f44d-$f484 (Code) ===
loc_f44d:
	ldy $a7                  ; $f44d: a4 a7
	cpy #$ff                 ; $f44f: c0 ff
	beq $f484                ; $f451: f0 31
loc_f453:
	lda $9e                  ; $f453: a5 9e
	and #$07                 ; $f455: 29 07 COLUP1
	bne $f484                ; $f457: d0 2b
loc_f459:
	iny                      ; $f459: c8
	sty $a7                  ; $f45a: 84 a7
	bit $94                  ; $f45c: 24 94
	bvs $f46c                ; $f45e: 70 0c
loc_f460:
	lda #$04                 ; $f460: a9 04 NUSIZ0
	sta $15                  ; $f462: 85 15 AUDC0
	sta $16                  ; $f464: 85 16 AUDC1
	lda #$0f                 ; $f466: a9 0f PF2
	sta $19                  ; $f468: 85 19 AUDV0
	sta $1a                  ; $f46a: 85 1a AUDV1
loc_f46c:
	lda $f485,y              ; $f46c: b9 85 f4
	cmp #$ff                 ; $f46f: c9 ff
	bne $f47d                ; $f471: d0 0a
loc_f473:
	sta $a7                  ; $f473: 85 a7
	lda #$00                 ; $f475: a9 00 VSYNC
	sta $19                  ; $f477: 85 19 AUDV0
	sta $1a                  ; $f479: 85 1a AUDV1
	beq $f484                ; $f47b: f0 07
loc_f47d:
	sta $17                  ; $f47d: 85 17 AUDF0
	lda $f4da,y              ; $f47f: b9 da f4
	sta $18                  ; $f482: 85 18 AUDF1
loc_f484:
	rts                      ; $f484: 60

; === Block $f52f-$f5a7 (Code) ===
loc_f52f:
	lda $8a                  ; $f52f: a5 8a
	ldy $90                  ; $f531: a4 90
	cmp #$55                 ; $f533: c9 55 AUDC0
	bmi $f543                ; $f535: 30 0c
loc_f537:
	cpy #$4f                 ; $f537: c0 4f PF2
	bmi $f53f                ; $f539: 30 04
loc_f53b:
	lda #$fb                 ; $f53b: a9 fb
	bmi $f54d                ; $f53d: 30 0e
loc_f53f:
	lda #$f7                 ; $f53f: a9 f7
	bmi $f54d                ; $f541: 30 0a
loc_f543:
	cpy #$4f                 ; $f543: c0 4f PF2
	bmi $f54b                ; $f545: 30 04
loc_f547:
	lda #$ef                 ; $f547: a9 ef
	bmi $f54d                ; $f549: 30 02
loc_f54b:
	lda #$df                 ; $f54b: a9 df
loc_f54d:
	and $f5                  ; $f54d: 25 f5
	sta $f5                  ; $f54f: 85 f5
	lda #$50                 ; $f551: a9 50 RESP0
	ldx #$00                 ; $f553: a2 00 VSYNC
	jsr $f31d                ; $f555: 20 1d f3
loc_f558:
	ldx #$03                 ; $f558: a2 03 RSYNC
	lda $81,x                ; $f55a: b5 81
	and #$70                 ; $f55c: 29 70
	cmp #$40                 ; $f55e: c9 40 VSYNC
	beq $f580                ; $f560: f0 1e
loc_f562:
	cmp #$60                 ; $f562: c9 60 HMP0
	beq $f580                ; $f564: f0 1a
loc_f566:
	cmp #$70                 ; $f566: c9 70
	beq $f5ac                ; $f568: f0 42
loc_f56a:
	lda $81,x                ; $f56a: b5 81
	and #$03                 ; $f56c: 29 03 RSYNC
	eor #$02                 ; $f56e: 49 02 WSYNC
	sta $81,x                ; $f570: 95 81
	lda $86,x                ; $f572: b5 86
	adc $8c,x                ; $f574: 75 8c
	eor $9e                  ; $f576: 45 9e
	and #$30                 ; $f578: 29 30
	ora $81,x                ; $f57a: 15 81
	ora #$80                 ; $f57c: 09 80
	sta $81,x                ; $f57e: 95 81
loc_f580:
	dex                      ; $f580: ca
	bpl $f55a                ; $f581: 10 d7
loc_f583:
	lda $f6                  ; $f583: a5 f6
	and #$78                 ; $f585: 29 78
	sta $f6                  ; $f587: 85 f6
	lda $92                  ; $f589: a5 92
	and #$f0                 ; $f58b: 29 f0
	sta $92                  ; $f58d: 85 92
	bit $f6                  ; $f58f: 24 f6
	bvc $f597                ; $f591: 50 04
loc_f593:
	lda #$08                 ; $f593: a9 08 COLUPF
	bne $f59d                ; $f595: d0 06
loc_f597:
	lda #$7f                 ; $f597: a9 7f
	sec                      ; $f599: 38
	sbc $fb                  ; $f59a: e5 fb
	lsr                      ; $f59c: 4a
loc_f59d:
	ldx $93                  ; $f59d: a6 93
	clc                      ; $f59f: 18
	adc $f5a8,x              ; $f5a0: 7d a8 f5
	sta $f4                  ; $f5a3: 85 f4
	inc $f7                  ; $f5a5: e6 f7
	rts                      ; $f5a7: 60

; === Block $f5ac-$f5ae (Code) ===
loc_f5ac:
	lda $81,x                ; $f5ac: b5 81
	jmp $f57c                ; $f5ae: 4c 7c f5

; === Block $f5b1-$f5e0 (Code) ===
loc_f5b1:
	lda $85                  ; $f5b1: a5 85
	and #$01                 ; $f5b3: 29 01 VBLANK
	bne $f5e1                ; $f5b5: d0 2a
loc_f5b7:
	lda $8a                  ; $f5b7: a5 8a
	cmp #$0c                 ; $f5b9: c9 0c REFP1
	bcc $f5f1                ; $f5bb: 90 34
loc_f5bd:
	sbc #$0c                 ; $f5bd: e9 0c REFP1
	cmp #$4e                 ; $f5bf: c9 4e PF1
	bpl $f5c9                ; $f5c1: 10 06
loc_f5c3:
	cmp #$4a                 ; $f5c3: c9 4a CTRLPF
	bpl $f5f1                ; $f5c5: 10 2a
loc_f5c7:
	adc #$05                 ; $f5c7: 69 05 NUSIZ1
loc_f5c9:
	adc #$01                 ; $f5c9: 69 01 VBLANK
	tax                      ; $f5cb: aa
	lsr                      ; $f5cc: 4a
	lsr                      ; $f5cd: 4a
	lsr                      ; $f5ce: 4a
	tay                      ; $f5cf: a8
	lda $90                  ; $f5d0: a5 90
	clc                      ; $f5d2: 18
	adc #$05                 ; $f5d3: 69 05 NUSIZ1
	jsr $fcf4                ; $f5d5: 20 f4 fc
loc_f5d8:
	cmp #$03                 ; $f5d8: c9 03 RSYNC
	bcc $f5f1                ; $f5da: 90 15
loc_f5dc:
	cmp #$08                 ; $f5dc: c9 08 COLUPF
	bcc $f60c                ; $f5de: 90 2c
loc_f5e0:
	rts                      ; $f5e0: 60

; === Block $f5e1-$f5f1 (Code) ===
loc_f5e1:
	lda $8a                  ; $f5e1: a5 8a
	cmp #$0c                 ; $f5e3: c9 0c REFP1
	bcc $f5f1                ; $f5e5: 90 0a
loc_f5e7:
	sbc #$0c                 ; $f5e7: e9 0c REFP1
	cmp #$4e                 ; $f5e9: c9 4e PF1
	bpl $f5f4                ; $f5eb: 10 07
loc_f5ed:
	cmp #$4a                 ; $f5ed: c9 4a CTRLPF
	bmi $f5f2                ; $f5ef: 30 01
loc_f5f1:
	rts                      ; $f5f1: 60

; === Block $f5f2-$f5f4 (Code) ===
loc_f5f2:
	adc #$05                 ; $f5f2: 69 05 NUSIZ1

; === Block $f5f4-$f60c (Code) ===
loc_f5f4:
	adc #$01                 ; $f5f4: 69 01 VBLANK
	tax                      ; $f5f6: aa
	lsr                      ; $f5f7: 4a
	lsr                      ; $f5f8: 4a
	lsr                      ; $f5f9: 4a
	beq $f5f1                ; $f5fa: f0 f5
loc_f5fc:
	cmp #$13                 ; $f5fc: c9 13 RESM1
	beq $f5f1                ; $f5fe: f0 f1
loc_f600:
	tay                      ; $f600: a8
	txa                      ; $f601: 8a
	and #$07                 ; $f602: 29 07 COLUP1
	cmp #$02                 ; $f604: c9 02 WSYNC
	bcc $f5f1                ; $f606: 90 e9
loc_f608:
	cmp #$06                 ; $f608: c9 06 COLUP0
	bcs $f5f1                ; $f60a: b0 e5

; === Block $f60c-$f656 (Code) ===
loc_f60c:
	lda $90                  ; $f60c: a5 90
	clc                      ; $f60e: 18
	adc #$05                 ; $f60f: 69 05 NUSIZ1
	lsr                      ; $f611: 4a
	lsr                      ; $f612: 4a
	tax                      ; $f613: aa
	lda $fde5,x              ; $f614: bd e5 fd
	sta $b7                  ; $f617: 85 b7
	asl                      ; $f619: 0a
	adc $b7                  ; $f61a: 65 b7
	tax                      ; $f61c: aa
	tya                      ; $f61d: 98
	cpy #$02                 ; $f61e: c0 02 WSYNC
	bmi $f636                ; $f620: 30 14
loc_f622:
	cpy #$06                 ; $f622: c0 06 COLUP0
	bmi $f635                ; $f624: 30 0f
loc_f626:
	cpy #$0a                 ; $f626: c0 0a CTRLPF
	bmi $f634                ; $f628: 30 0a
loc_f62a:
	cpy #$0e                 ; $f62a: c0 0e PF1
	bmi $f635                ; $f62c: 30 07
loc_f62e:
	cpy #$12                 ; $f62e: c0 12 RESM0
	bmi $f634                ; $f630: 30 02
loc_f632:
	bpl $f636                ; $f632: 10 02
loc_f634:
	inx                      ; $f634: e8
loc_f635:
	inx                      ; $f635: e8
loc_f636:
	lda $bb,x                ; $f636: b5 bb
	dey                      ; $f638: 88
	and $ffb3,y              ; $f639: 39 b3 ff
	beq $f656                ; $f63c: f0 18
loc_f63e:
	lda $bb,x                ; $f63e: b5 bb
	eor $ffb3,y              ; $f640: 59 b3 ff
	sta $bb,x                ; $f643: 95 bb
	lda #$10                 ; $f645: a9 10 RESP0
	ldx #$00                 ; $f647: a2 00 VSYNC
	jsr $f31d                ; $f649: 20 1d f3
loc_f64c:
	lda #$c0                 ; $f64c: a9 c0
	ora $85                  ; $f64e: 05 85
	and #$cf                 ; $f650: 29 cf
	sta $85                  ; $f652: 85 85
	inc $f7                  ; $f654: e6 f7
loc_f656:
	rts                      ; $f656: 60

; === Block $f657-$f69b (Code) ===
loc_f657:
	lda $8b                  ; $f657: a5 8b
	beq $f69e                ; $f659: f0 43
loc_f65b:
	lda $9f                  ; $f65b: a5 9f
	and #$1f                 ; $f65d: 29 1f ENABL
	sta $b9                  ; $f65f: 85 b9
	lda $85                  ; $f661: a5 85
	lsr                      ; $f663: 4a
	lsr                      ; $f664: 4a
	and #$03                 ; $f665: 29 03 RSYNC
	sta $ba                  ; $f667: 85 ba
	lda $f6                  ; $f669: a5 f6
	tax                      ; $f66b: aa
	and #$c7                 ; $f66c: 29 c7
	sta $f6                  ; $f66e: 85 f6
	txa                      ; $f670: 8a
	clc                      ; $f671: 18
	adc #$08                 ; $f672: 69 08 COLUPF
	and #$38                 ; $f674: 29 38
	ora $f6                  ; $f676: 05 f6
	sta $f6                  ; $f678: 85 f6
	and #$38                 ; $f67a: 29 38
	lsr                      ; $f67c: 4a
	lsr                      ; $f67d: 4a
	lsr                      ; $f67e: 4a
	sta $b8                  ; $f67f: 85 b8
	cmp #$05                 ; $f681: c9 05 NUSIZ1
	bne $f695                ; $f683: d0 10
loc_f685:
	bit $94                  ; $f685: 24 94
	bvs $f695                ; $f687: 70 0c
loc_f689:
	lda #$0f                 ; $f689: a9 0f PF2
	sta $19                  ; $f68b: 85 19 AUDV0
	lda #$05                 ; $f68d: a9 05 NUSIZ1
	sta $15                  ; $f68f: 85 15 AUDC0
	lda #$10                 ; $f691: a9 10 RESP0
	sta $17                  ; $f693: 85 17 AUDF0
loc_f695:
	lda #$20                 ; $f695: a9 20 HMP0
	bit $9f                  ; $f697: 24 9f
	bpl $f708                ; $f699: 10 6d
loc_f69b:
	jmp $f751                ; $f69b: 4c 51 f7

; === Block $f69e-$f6aa (Code) ===
loc_f69e:
	ldx $f7                  ; $f69e: a6 f7
	lda #$20                 ; $f6a0: a9 20 HMP0
	bit $9f                  ; $f6a2: 24 9f
	bne $f6ab                ; $f6a4: d0 05
loc_f6a6:
	cpx #$32                 ; $f6a6: e0 32
	beq $f6b0                ; $f6a8: f0 06
loc_f6aa:
	rts                      ; $f6aa: 60

; === Block $f6ab-$f6af (Code) ===
loc_f6ab:
	cpx #$64                 ; $f6ab: e0 64 HMBL
	bpl $f6b0                ; $f6ad: 10 01
loc_f6af:
	rts                      ; $f6af: 60

; === Block $f6b0-$f6d0 (Code) ===
loc_f6b0:
	lda $9e                  ; $f6b0: a5 9e
	lsr                      ; $f6b2: 4a
	lsr                      ; $f6b3: 4a
	adc $90                  ; $f6b4: 65 90
	eor $8a                  ; $f6b6: 45 8a
	eor $80                  ; $f6b8: 45 80
	and #$03                 ; $f6ba: 29 03 RSYNC
	sta $ba                  ; $f6bc: 85 ba
	beq $f6d3                ; $f6be: f0 13
loc_f6c0:
	cmp #$03                 ; $f6c0: c9 03 RSYNC
	beq $f6d3                ; $f6c2: f0 0f
loc_f6c4:
	lda #$01                 ; $f6c4: a9 01 VBLANK
	sta $b8                  ; $f6c6: 85 b8
	lda $f6                  ; $f6c8: a5 f6
	and #$c7                 ; $f6ca: 29 c7
	ora #$08                 ; $f6cc: 09 08 COLUPF
	sta $f6                  ; $f6ce: 85 f6
	jmp $f6dd                ; $f6d0: 4c dd f6

; === Block $f6d3-$f705 (Code) ===
loc_f6d3:
	lda #$02                 ; $f6d3: a9 02 WSYNC
	sta $b8                  ; $f6d5: 85 b8
	lda $f6                  ; $f6d7: a5 f6
	and #$c7                 ; $f6d9: 29 c7
	ora #$10                 ; $f6db: 09 10 RESP0
loc_f6dd:
	sta $f6                  ; $f6dd: 85 f6
	lda $9f                  ; $f6df: a5 9f
	eor #$ff                 ; $f6e1: 49 ff
	sta $9f                  ; $f6e3: 85 9f
	lda $80                  ; $f6e5: a5 80
	asl                      ; $f6e7: 0a
	asl                      ; $f6e8: 0a
	clc                      ; $f6e9: 18
	adc $ba                  ; $f6ea: 65 ba
	tay                      ; $f6ec: a8
	lda $fd52,y              ; $f6ed: b9 52 fd
	sta $91                  ; $f6f0: 85 91
	ldy $ba                  ; $f6f2: a4 ba
	lda $fd4e,y              ; $f6f4: b9 4e fd
	sta $8b                  ; $f6f7: 85 8b
	lda #$00                 ; $f6f9: a9 00 VSYNC
	sta $b9                  ; $f6fb: 85 b9
	lda $9f                  ; $f6fd: a5 9f
	and #$20                 ; $f6ff: 29 20 HMP0
	ora #$40                 ; $f701: 09 40 VSYNC
	sta $9f                  ; $f703: 85 9f
	jmp $f7d1                ; $f705: 4c d1 f7

; === Block $f708-$f72e (Code) ===
loc_f708:
	lda $8b                  ; $f708: a5 8b
	cmp #$60                 ; $f70a: c9 60 HMP0
	bpl $f716                ; $f70c: 10 08
loc_f70e:
	lda $b8                  ; $f70e: a5 b8
	cmp #$02                 ; $f710: c9 02 WSYNC
	bne $f74e                ; $f712: d0 3a
loc_f714:
	beq $f71c                ; $f714: f0 06
loc_f716:
	lda $b8                  ; $f716: a5 b8
	cmp #$06                 ; $f718: c9 06 COLUP0
	bne $f74e                ; $f71a: d0 32
loc_f71c:
	lda #$20                 ; $f71c: a9 20 HMP0
	bit $9f                  ; $f71e: 24 9f
	bvs $f72f                ; $f720: 70 0d
loc_f722:
	dec $b9                  ; $f722: c6 b9
	lda $b9                  ; $f724: a5 b9
	cmp #$ff                 ; $f726: c9 ff
	bne $f74e                ; $f728: d0 24
loc_f72a:
	lda #$00                 ; $f72a: a9 00 VSYNC
	sta $8b                  ; $f72c: 85 8b
	rts                      ; $f72e: 60

; === Block $f72f-$f74b (Code) ===
loc_f72f:
	inc $b9                  ; $f72f: e6 b9
	jsr $f7b2                ; $f731: 20 b2 f7
loc_f734:
	cmp $b9                  ; $f734: c5 b9
	bcs $f74e                ; $f736: b0 16
loc_f738:
	lda $9f                  ; $f738: a5 9f
	ora #$80                 ; $f73a: 09 80
	sta $9f                  ; $f73c: 85 9f
	lda $80                  ; $f73e: a5 80
	asl                      ; $f740: 0a
	asl                      ; $f741: 0a
	clc                      ; $f742: 18
	adc $ba                  ; $f743: 65 ba
	tax                      ; $f745: aa
	lda $fd3e,x              ; $f746: bd 3e fd
	sta $b9                  ; $f749: 85 b9
	jmp $f81a                ; $f74b: 4c 1a f8

; === Block $f74e-$f74e (Code) ===
loc_f74e:
	jmp $f7d1                ; $f74e: 4c d1 f7

; === Block $f751-$f797 (Code) ===
loc_f751:
	lda $8b                  ; $f751: a5 8b
	cmp #$60                 ; $f753: c9 60 HMP0
	bpl $f75f                ; $f755: 10 08
loc_f757:
	lda $b8                  ; $f757: a5 b8
	cmp #$02                 ; $f759: c9 02 WSYNC
	bne $f7af                ; $f75b: d0 52
loc_f75d:
	beq $f765                ; $f75d: f0 06
loc_f75f:
	lda $b8                  ; $f75f: a5 b8
	cmp #$06                 ; $f761: c9 06 COLUP0
	bne $f7af                ; $f763: d0 4a
loc_f765:
	ldx $b9                  ; $f765: a6 b9
	inx                      ; $f767: e8
	cpx #$12                 ; $f768: e0 12 RESM0
	bne $f76e                ; $f76a: d0 02
loc_f76c:
	ldx #$00                 ; $f76c: a2 00 VSYNC
loc_f76e:
	stx $b9                  ; $f76e: 86 b9
	lda $80                  ; $f770: a5 80
	asl                      ; $f772: 0a
	asl                      ; $f773: 0a
	clc                      ; $f774: 18
	adc $ba                  ; $f775: 65 ba
	tax                      ; $f777: aa
	lda #$20                 ; $f778: a9 20 HMP0
	bit $9f                  ; $f77a: 24 9f
	bvc $f79a                ; $f77c: 50 1c
loc_f77e:
	lda $fd2e,x              ; $f77e: bd 2e fd
	cmp $b9                  ; $f781: c5 b9
	bne $f7af                ; $f783: d0 2a
loc_f785:
	lda $90                  ; $f785: a5 90
	adc $8a                  ; $f787: 65 8a
	eor $86                  ; $f789: 45 86
	eor $8f                  ; $f78b: 45 8f
	and #$03                 ; $f78d: 29 03 RSYNC
	sta $ba                  ; $f78f: 85 ba
	lda $9f                  ; $f791: a5 9f
	and #$bf                 ; $f793: 29 bf
	sta $9f                  ; $f795: 85 9f
	jmp $f7af                ; $f797: 4c af f7

; === Block $f79a-$f7ac (Code) ===
loc_f79a:
	lda $fd3e,x              ; $f79a: bd 3e fd
	cmp $b9                  ; $f79d: c5 b9
	bne $f7af                ; $f79f: d0 0e
loc_f7a1:
	lda $9f                  ; $f7a1: a5 9f
	and #$7f                 ; $f7a3: 29 7f
	sta $9f                  ; $f7a5: 85 9f
	jsr $f7b2                ; $f7a7: 20 b2 f7
loc_f7aa:
	sta $b9                  ; $f7aa: 85 b9
	jmp $f7d1                ; $f7ac: 4c d1 f7

; === Block $f7af-$f7af (Code) ===
loc_f7af:
	jmp $f81a                ; $f7af: 4c 1a f8

; === Block $f7b2-$f7c8 (Code) ===
loc_f7b2:
	lda $80                  ; $f7b2: a5 80
	asl                      ; $f7b4: 0a
	sta $b7                  ; $f7b5: 85 b7
	lda $ba                  ; $f7b7: a5 ba
	lsr                      ; $f7b9: 4a
	clc                      ; $f7ba: 18
	adc $b7                  ; $f7bb: 65 b7
	tax                      ; $f7bd: aa
	lda $ba                  ; $f7be: a5 ba
	lsr                      ; $f7c0: 4a
	bcc $f7c9                ; $f7c1: 90 06
loc_f7c3:
	lda $fd62,x              ; $f7c3: bd 62 fd
	and #$0f                 ; $f7c6: 29 0f PF2
	rts                      ; $f7c8: 60

; === Block $f7c9-$f7d0 (Code) ===
loc_f7c9:
	lda $fd62,x              ; $f7c9: bd 62 fd
	lsr                      ; $f7cc: 4a
	lsr                      ; $f7cd: 4a
	lsr                      ; $f7ce: 4a
	lsr                      ; $f7cf: 4a
	rts                      ; $f7d0: 60

; === Block $f7d1-$f817 (Code) ===
loc_f7d1:
	lda $b9                  ; $f7d1: a5 b9
	and #$03                 ; $f7d3: 29 03 RSYNC
	eor #$ff                 ; $f7d5: 49 ff
	clc                      ; $f7d7: 18
	adc #$04                 ; $f7d8: 69 04 NUSIZ0
	tay                      ; $f7da: a8
	lda $b9                  ; $f7db: a5 b9
	lsr                      ; $f7dd: 4a
	lsr                      ; $f7de: 4a
	sta $b7                  ; $f7df: 85 b7
	lda $80                  ; $f7e1: a5 80
	asl                      ; $f7e3: 0a
	asl                      ; $f7e4: 0a
	asl                      ; $f7e5: 0a
	asl                      ; $f7e6: 0a
	clc                      ; $f7e7: 18
	adc $b7                  ; $f7e8: 65 b7
	sta $b7                  ; $f7ea: 85 b7
	lda $ba                  ; $f7ec: a5 ba
	asl                      ; $f7ee: 0a
	asl                      ; $f7ef: 0a
	clc                      ; $f7f0: 18
	adc $b7                  ; $f7f1: 65 b7
	tax                      ; $f7f3: aa
	lda $fd6a,x              ; $f7f4: bd 6a fd
	cpy #$00                 ; $f7f7: c0 00 VSYNC
	beq $f800                ; $f7f9: f0 05
loc_f7fb:
	lsr                      ; $f7fb: 4a
	lsr                      ; $f7fc: 4a
	dey                      ; $f7fd: 88
	bne $f7fb                ; $f7fe: d0 fb
loc_f800:
	sta $99                  ; $f800: 85 99
	lda #$20                 ; $f802: a9 20 HMP0
	bit $9f                  ; $f804: 24 9f
	bvs $f817                ; $f806: 70 0f
loc_f808:
	lda $99                  ; $f808: a5 99
	eor #$02                 ; $f80a: 49 02 WSYNC
	sta $99                  ; $f80c: 85 99
	lda $b8                  ; $f80e: a5 b8
	eor #$ff                 ; $f810: 49 ff
	clc                      ; $f812: 18
	adc #$08                 ; $f813: 69 08 COLUPF
	sta $b8                  ; $f815: 85 b8
loc_f817:
	jmp $f837                ; $f817: 4c 37 f8

; === Block $f81a-$f851 (Code) ===
loc_f81a:
	lda $b9                  ; $f81a: a5 b9
	and #$03                 ; $f81c: 29 03 RSYNC
	eor #$ff                 ; $f81e: 49 ff
	clc                      ; $f820: 18
	adc #$04                 ; $f821: 69 04 NUSIZ0
	tay                      ; $f823: a8
	lda $b9                  ; $f824: a5 b9
	lsr                      ; $f826: 4a
	lsr                      ; $f827: 4a
	tax                      ; $f828: aa
	lda $fdaa,x              ; $f829: bd aa fd
	cpy #$00                 ; $f82c: c0 00 VSYNC
	beq $f835                ; $f82e: f0 05
loc_f830:
	lsr                      ; $f830: 4a
	lsr                      ; $f831: 4a
	dey                      ; $f832: 88
	bne $f830                ; $f833: d0 fb
loc_f835:
	sta $99                  ; $f835: 85 99
loc_f837:
	ldy $b8                  ; $f837: a4 b8
	lda $99                  ; $f839: a5 99
	lsr                      ; $f83b: 4a
	bcc $f866                ; $f83c: 90 28
loc_f83e:
	lsr                      ; $f83e: 4a
	bcc $f854                ; $f83f: 90 13
loc_f841:
	lda #$ff                 ; $f841: a9 ff
	clc                      ; $f843: 18
	adc $8b                  ; $f844: 65 8b
	cmp #$0b                 ; $f846: c9 0b REFP0
	bne $f84c                ; $f848: d0 02
loc_f84a:
	lda #$00                 ; $f84a: a9 00 VSYNC
loc_f84c:
	sta $8b                  ; $f84c: 85 8b
	lda $fdb2,y              ; $f84e: b9 b2 fd
	jmp $f872                ; $f851: 4c 72 f8

; === Block $f854-$f863 (Code) ===
loc_f854:
	lda #$01                 ; $f854: a9 01 VBLANK
	adc $8b                  ; $f856: 65 8b
	cmp #$a4                 ; $f858: c9 a4
	bne $f85e                ; $f85a: d0 02
loc_f85c:
	lda #$00                 ; $f85c: a9 00 VSYNC
loc_f85e:
	sta $8b                  ; $f85e: 85 8b
	lda $fdb2,y              ; $f860: b9 b2 fd
	jmp $f872                ; $f863: 4c 72 f8

; === Block $f866-$f86c (Code) ===
loc_f866:
	lsr                      ; $f866: 4a
	bcc $f86f                ; $f867: 90 06
loc_f869:
	lda $fdba,y              ; $f869: b9 ba fd
	jmp $f872                ; $f86c: 4c 72 f8

; === Block $f86f-$f872 (Code) ===
loc_f86f:
	lda $fdc2,y              ; $f86f: b9 c2 fd

; === Block $f872-$f899 (Code) ===
loc_f872:
	ldx $9f                  ; $f872: a6 9f
	cpx #$00                 ; $f874: e0 00 VSYNC
	bmi $f87e                ; $f876: 30 06
loc_f878:
	ldx $b9                  ; $f878: a6 b9
	cpx #$00                 ; $f87a: e0 00 VSYNC
	beq $f883                ; $f87c: f0 05
loc_f87e:
	clc                      ; $f87e: 18
	adc $91                  ; $f87f: 65 91
	sta $91                  ; $f881: 85 91
loc_f883:
	lda $85                  ; $f883: a5 85
	and #$f3                 ; $f885: 29 f3
	sta $85                  ; $f887: 85 85
	lda $ba                  ; $f889: a5 ba
	asl                      ; $f88b: 0a
	asl                      ; $f88c: 0a
	ora $85                  ; $f88d: 05 85
	sta $85                  ; $f88f: 85 85
	lda $9f                  ; $f891: a5 9f
	and #$e0                 ; $f893: 29 e0
	ora $b9                  ; $f895: 05 b9
	sta $9f                  ; $f897: 85 9f
	rts                      ; $f899: 60

; === Block $f89a-$f8a6 (Code) ===
loc_f89a:
	cpy #$04                 ; $f89a: c0 04 NUSIZ0
	beq $f8b3                ; $f89c: f0 15
loc_f89e:
	lda $0081,y              ; $f89e: b9 81 00
	tax                      ; $f8a1: aa
	and #$04                 ; $f8a2: 29 04 NUSIZ0
	beq $f8a9                ; $f8a4: f0 03
loc_f8a6:
	jmp $f94b                ; $f8a6: 4c 4b f9

; === Block $f8a9-$f8b0 (Code) ===
loc_f8a9:
	txa                      ; $f8a9: 8a
	and #$7f                 ; $f8aa: 29 7f
	lsr                      ; $f8ac: 4a
	lsr                      ; $f8ad: 4a
	lsr                      ; $f8ae: 4a
	lsr                      ; $f8af: 4a
	jmp $f8c0                ; $f8b0: 4c c0 f8

; === Block $f8b3-$f8b9 (Code) ===
loc_f8b3:
	lda #$20                 ; $f8b3: a9 20 HMP0
	bit $94                  ; $f8b5: 24 94
	beq $f8bc                ; $f8b7: f0 03
loc_f8b9:
	jmp $f99e                ; $f8b9: 4c 9e f9

; === Block $f8bc-$f92f (Code) ===
loc_f8bc:
	lda $94                  ; $f8bc: a5 94
	and #$03                 ; $f8be: 29 03 RSYNC
loc_f8c0:
	tax                      ; $f8c0: aa
	cmp #$05                 ; $f8c1: c9 05 NUSIZ1
	beq $f8ca                ; $f8c3: f0 05
loc_f8c5:
	lda $fdca,x              ; $f8c5: bd ca fd
	bne $f8cc                ; $f8c8: d0 02
loc_f8ca:
	lda $8a                  ; $f8ca: a5 8a
loc_f8cc:
	sec                      ; $f8cc: 38
	sbc $0086,y              ; $f8cd: f9 86 00
	sta $ba                  ; $f8d0: 85 ba
	bcc $f8d8                ; $f8d2: 90 04
loc_f8d4:
	lda #$01                 ; $f8d4: a9 01 VBLANK
	bne $f8e1                ; $f8d6: d0 09
loc_f8d8:
	eor #$ff                 ; $f8d8: 49 ff
	clc                      ; $f8da: 18
	adc #$01                 ; $f8db: 69 01 VBLANK
	sta $ba                  ; $f8dd: 85 ba
	lda #$03                 ; $f8df: a9 03 RSYNC
loc_f8e1:
	sta $b9                  ; $f8e1: 85 b9
	lda $ba                  ; $f8e3: a5 ba
	lsr                      ; $f8e5: 4a
	clc                      ; $f8e6: 18
	adc $ba                  ; $f8e7: 65 ba
	sta $ba                  ; $f8e9: 85 ba
	cpx #$05                 ; $f8eb: e0 05 NUSIZ1
	beq $f8f4                ; $f8ed: f0 05
loc_f8ef:
	lda $fdcf,x              ; $f8ef: bd cf fd
	bne $f8f6                ; $f8f2: d0 02
loc_f8f4:
	lda $90                  ; $f8f4: a5 90
loc_f8f6:
	sec                      ; $f8f6: 38
	sbc $008c,y              ; $f8f7: f9 8c 00
	sta $99                  ; $f8fa: 85 99
	beq $f917                ; $f8fc: f0 19
loc_f8fe:
	bcc $f908                ; $f8fe: 90 08
loc_f900:
	lda $b9                  ; $f900: a5 b9
	ora #$08                 ; $f902: 09 08 COLUPF
	sta $b9                  ; $f904: 85 b9
	bne $f90f                ; $f906: d0 07
loc_f908:
	eor #$ff                 ; $f908: 49 ff
	clc                      ; $f90a: 18
	adc #$01                 ; $f90b: 69 01 VBLANK
	sta $99                  ; $f90d: 85 99
loc_f90f:
	lda $99                  ; $f90f: a5 99
	cmp $ba                  ; $f911: c5 ba
	bcc $f92f                ; $f913: 90 1a
loc_f915:
	bcs $f929                ; $f915: b0 12
loc_f917:
	lda $ba                  ; $f917: a5 ba
	beq $f930                ; $f919: f0 15
loc_f91b:
	lda $9e                  ; $f91b: a5 9e
	lsr                      ; $f91d: 4a
	lsr                      ; $f91e: 4a
	bcs $f92f                ; $f91f: b0 0e
loc_f921:
	lda $b9                  ; $f921: a5 b9
	ora #$08                 ; $f923: 09 08 COLUPF
	sta $b9                  ; $f925: 85 b9
	bne $f92f                ; $f927: d0 06
loc_f929:
	lda $b9                  ; $f929: a5 b9
	ora #$80                 ; $f92b: 09 80
	sta $b9                  ; $f92d: 85 b9
loc_f92f:
	rts                      ; $f92f: 60

; === Block $f930-$f934 (Code) ===
loc_f930:
	cpy #$04                 ; $f930: c0 04 NUSIZ0
	bne $f937                ; $f932: d0 03
loc_f934:
	jmp $f996                ; $f934: 4c 96 f9

; === Block $f937-$f94b (Code) ===
loc_f937:
	lda $0081,y              ; $f937: b9 81 00
	and #$83                 ; $f93a: 29 83
	ora #$04                 ; $f93c: 09 04 NUSIZ0
	sta $b9                  ; $f93e: 85 b9
	lda $9e                  ; $f940: a5 9e
	and #$38                 ; $f942: 29 38
	ora $b9                  ; $f944: 05 b9
	sta $0081,y              ; $f946: 99 81 00
	bne $f98b                ; $f949: d0 40

; === Block $f94b-$f995 (Code) ===
loc_f94b:
	lda $0081,y              ; $f94b: b9 81 00
	tax                      ; $f94e: aa
	and #$38                 ; $f94f: 29 38
	beq $f95e                ; $f951: f0 0b
loc_f953:
	lda $0081,y              ; $f953: b9 81 00
	sec                      ; $f956: 38
	sbc #$08                 ; $f957: e9 08 COLUPF
	sta $0081,y              ; $f959: 99 81 00
	bne $f98b                ; $f95c: d0 2d
loc_f95e:
	lda $0081,y              ; $f95e: b9 81 00
	and #$83                 ; $f961: 29 83
	ora #$50                 ; $f963: 09 50 RESP0
	sta $0081,y              ; $f965: 99 81 00
	lda $f6                  ; $f968: a5 f6
	and #$78                 ; $f96a: 29 78
	sta $f6                  ; $f96c: 85 f6
	lda $fdd4,y              ; $f96e: b9 d4 fd
	ora $92                  ; $f971: 05 92
	sta $92                  ; $f973: 85 92
	and #$0f                 ; $f975: 29 0f PF2
	cmp #$0f                 ; $f977: c9 0f PF2
	bne $f98b                ; $f979: d0 10
loc_f97b:
	lda $90                  ; $f97b: a5 90
	adc $9e                  ; $f97d: 65 9e
	eor $f7                  ; $f97f: 45 f7
	eor $8a                  ; $f981: 45 8a
	and #$07                 ; $f983: 29 07 COLUP1
	ora $f6                  ; $f985: 05 f6
	ora #$80                 ; $f987: 09 80
	sta $f6                  ; $f989: 85 f6
loc_f98b:
	lda $9e                  ; $f98b: a5 9e
	and #$0f                 ; $f98d: 29 0f PF2
	tax                      ; $f98f: aa
	lda $fd1e,x              ; $f990: bd 1e fd
	sta $b9                  ; $f993: 85 b9
	rts                      ; $f995: 60

; === Block $f996-$f99e (Code) ===
loc_f996:
	lda $94                  ; $f996: a5 94
	and #$c0                 ; $f998: 29 c0
	ora #$2f                 ; $f99a: 09 2f
	sta $94                  ; $f99c: 85 94

; === Block $f99e-$f9b7 (Code) ===
loc_f99e:
	lda $94                  ; $f99e: a5 94
	sec                      ; $f9a0: 38
	sbc #$01                 ; $f9a1: e9 01 VBLANK
	sta $94                  ; $f9a3: 85 94
	and #$20                 ; $f9a5: 29 20 HMP0
	beq $f9b8                ; $f9a7: f0 0f
loc_f9a9:
	lda $9e                  ; $f9a9: a5 9e
	lsr                      ; $f9ab: 4a
	lsr                      ; $f9ac: 4a
	lsr                      ; $f9ad: 4a
	lsr                      ; $f9ae: 4a
	and #$0f                 ; $f9af: 29 0f PF2
	tax                      ; $f9b1: aa
	lda $fd1e,x              ; $f9b2: bd 1e fd
	sta $b9                  ; $f9b5: 85 b9
	rts                      ; $f9b7: 60

; === Block $f9b8-$f9cd (Code) ===
loc_f9b8:
	lda $94                  ; $f9b8: a5 94
	and #$c0                 ; $f9ba: 29 c0
	sta $94                  ; $f9bc: 85 94
	lda $9e                  ; $f9be: a5 9e
	lsr                      ; $f9c0: 4a
	lsr                      ; $f9c1: 4a
	lsr                      ; $f9c2: 4a
	and #$03                 ; $f9c3: 29 03 RSYNC
	ora $94                  ; $f9c5: 05 94
	sta $94                  ; $f9c7: 85 94
	lda #$81                 ; $f9c9: a9 81
	sta $b9                  ; $f9cb: 85 b9
	rts                      ; $f9cd: 60

; === Block $f9ce-$f9e9 (Code) ===
loc_f9ce:
	cpy #$04                 ; $f9ce: c0 04 NUSIZ0
	beq $fa1f                ; $f9d0: f0 4d
loc_f9d2:
	lda $0086,y              ; $f9d2: b9 86 00
	cmp #$12                 ; $f9d5: c9 12 RESM0
	bcc $f9dd                ; $f9d7: 90 04
loc_f9d9:
	cmp #$ab                 ; $f9d9: c9 ab
	bcc $f9ea                ; $f9db: 90 0d
loc_f9dd:
	lda $fd1a,y              ; $f9dd: b9 1a fd
	eor $92                  ; $f9e0: 45 92
	sta $92                  ; $f9e2: 85 92
	and $fd1a,y              ; $f9e4: 39 1a fd
	beq $f9ea                ; $f9e7: f0 01
loc_f9e9:
	rts                      ; $f9e9: 60

; === Block $f9ea-$f9fb (Code) ===
loc_f9ea:
	lda $0081,y              ; $f9ea: b9 81 00
	and #$70                 ; $f9ed: 29 70
	cmp #$40                 ; $f9ef: c9 40 VSYNC
	beq $f9fe                ; $f9f1: f0 0b
loc_f9f3:
	cmp #$60                 ; $f9f3: c9 60 HMP0
	beq $fa1c                ; $f9f5: f0 25
loc_f9f7:
	cmp #$70                 ; $f9f7: c9 70
	bne $fa1f                ; $f9f9: d0 24
loc_f9fb:
	jmp $fb84                ; $f9fb: 4c 84 fb

; === Block $f9fe-$fa1c (Code) ===
loc_f9fe:
	lda $f6                  ; $f9fe: a5 f6
	and #$78                 ; $fa00: 29 78
	sta $f6                  ; $fa02: 85 f6
	lda $0086,y              ; $fa04: b9 86 00
	cmp #$58                 ; $fa07: c9 58 AUDF1
	bne $fa1f                ; $fa09: d0 14
loc_fa0b:
	lda $008c,y              ; $fa0b: b9 8c 00
	cmp #$32                 ; $fa0e: c9 32
	bne $fa1f                ; $fa10: d0 0d
loc_fa12:
	lda $0081,y              ; $fa12: b9 81 00
	and #$8f                 ; $fa15: 29 8f
	ora #$60                 ; $fa17: 09 60 HMP0
	sta $0081,y              ; $fa19: 99 81 00
loc_fa1c:
	jmp $fb64                ; $fa1c: 4c 64 fb

; === Block $fa1f-$fa56 (Code) ===
loc_fa1f:
	lda $0081,y              ; $fa1f: b9 81 00
	lsr                      ; $fa22: 4a
	bcc $fa48                ; $fa23: 90 23
loc_fa25:
	lda $0086,y              ; $fa25: b9 86 00
	cmp #$58                 ; $fa28: c9 58 AUDF1
	bcc $fa38                ; $fa2a: 90 0c
loc_fa2c:
	cmp #$60                 ; $fa2c: c9 60 HMP0
	bcs $fa40                ; $fa2e: b0 10
loc_fa30:
	and #$07                 ; $fa30: 29 07 COLUP1
	cmp #$00                 ; $fa32: c9 00 VSYNC
	bne $fa52                ; $fa34: d0 1c
loc_fa36:
	beq $fa73                ; $fa36: f0 3b
loc_fa38:
	and #$07                 ; $fa38: 29 07 COLUP1
	cmp #$02                 ; $fa3a: c9 02 WSYNC
	bne $fa52                ; $fa3c: d0 14
loc_fa3e:
	beq $fa73                ; $fa3e: f0 33
loc_fa40:
	and #$07                 ; $fa40: 29 07 COLUP1
	cmp #$06                 ; $fa42: c9 06 COLUP0
	bne $fa52                ; $fa44: d0 0c
loc_fa46:
	beq $fa73                ; $fa46: f0 2b
loc_fa48:
	lda $008c,y              ; $fa48: b9 8c 00
	jsr $fcf4                ; $fa4b: 20 f4 fc
loc_fa4e:
	cmp #$02                 ; $fa4e: c9 02 WSYNC
	beq $fa73                ; $fa50: f0 21
loc_fa52:
	cpy #$04                 ; $fa52: c0 04 NUSIZ0
	beq $fa59                ; $fa54: f0 03
loc_fa56:
	jmp $fb35                ; $fa56: 4c 35 fb

; === Block $fa59-$fa5d (Code) ===
loc_fa59:
	bit $94                  ; $fa59: 24 94
	bvc $fa60                ; $fa5b: 50 03
loc_fa5d:
	jmp $fb35                ; $fa5d: 4c 35 fb

; === Block $fa60-$fa6d (Code) ===
loc_fa60:
	jsr $fc22                ; $fa60: 20 22 fc
loc_fa63:
	ldy $b8                  ; $fa63: a4 b8
	lda $99                  ; $fa65: a5 99
	eor $ba                  ; $fa67: 45 ba
	cmp #$02                 ; $fa69: c9 02 WSYNC
	beq $fa70                ; $fa6b: f0 03
loc_fa6d:
	jmp $fb35                ; $fa6d: 4c 35 fb

; === Block $fa70-$fa70 (Code) ===
loc_fa70:
	jmp $fb2b                ; $fa70: 4c 2b fb

; === Block $fa73-$fa77 (Code) ===
loc_fa73:
	dec $96                  ; $fa73: c6 96
	bpl $fa78                ; $fa75: 10 01
loc_fa77:
	rts                      ; $fa77: 60

; === Block $fa78-$fa8c (Code) ===
loc_fa78:
	ldx $86,y                ; $fa78: b6 86
	lda $008c,y              ; $fa7a: b9 8c 00
	jsr $fc72                ; $fa7d: 20 72 fc
loc_fa80:
	sta $b7                  ; $fa80: 85 b7
	ldy $b8                  ; $fa82: a4 b8
	cpy #$04                 ; $fa84: c0 04 NUSIZ0
	bne $fa8f                ; $fa86: d0 07
loc_fa88:
	bit $94                  ; $fa88: 24 94
	bvs $fa8f                ; $fa8a: 70 03
loc_fa8c:
	jmp $fb13                ; $fa8c: 4c 13 fb

; === Block $fa8f-$fab4 (Code) ===
loc_fa8f:
	jsr $f89a                ; $fa8f: 20 9a f8
loc_fa92:
	lda $0081,y              ; $fa92: b9 81 00
	and #$03                 ; $fa95: 29 03 RSYNC
	sta $ba                  ; $fa97: 85 ba
	lda $b9                  ; $fa99: a5 b9
	bpl $fa9f                ; $fa9b: 10 02
loc_fa9d:
	lsr                      ; $fa9d: 4a
	lsr                      ; $fa9e: 4a
loc_fa9f:
	and #$03                 ; $fa9f: 29 03 RSYNC
	sta $99                  ; $faa1: 85 99
	eor $ba                  ; $faa3: 45 ba
	beq $facc                ; $faa5: f0 25
loc_faa7:
	cmp #$02                 ; $faa7: c9 02 WSYNC
	beq $faf0                ; $faa9: f0 45
loc_faab:
	ldx $99                  ; $faab: a6 99
	lda $fdd4,x              ; $faad: bd d4 fd
	and $b7                  ; $fab0: 25 b7
	beq $fab7                ; $fab2: f0 03
loc_fab4:
	jmp $fb2b                ; $fab4: 4c 2b fb

; === Block $fab7-$fac0 (Code) ===
loc_fab7:
	ldx $ba                  ; $fab7: a6 ba
	lda $fdd4,x              ; $fab9: bd d4 fd
	and $b7                  ; $fabc: 25 b7
	beq $fac3                ; $fabe: f0 03
loc_fac0:
	jmp $fb35                ; $fac0: 4c 35 fb

; === Block $fac3-$fac9 (Code) ===
loc_fac3:
	lda $99                  ; $fac3: a5 99
	eor #$02                 ; $fac5: 49 02 WSYNC
	sta $99                  ; $fac7: 85 99
	jmp $fb2b                ; $fac9: 4c 2b fb

; === Block $facc-$faed (Code) ===
loc_facc:
	ldx $ba                  ; $facc: a6 ba
	lda $fdd4,x              ; $face: bd d4 fd
	and $b7                  ; $fad1: 25 b7
	bne $fb35                ; $fad3: d0 60
loc_fad5:
	lda $b9                  ; $fad5: a5 b9
	bmi $fadb                ; $fad7: 30 02
loc_fad9:
	lsr                      ; $fad9: 4a
	lsr                      ; $fada: 4a
loc_fadb:
	and #$03                 ; $fadb: 29 03 RSYNC
	sta $99                  ; $fadd: 85 99
	tax                      ; $fadf: aa
	lda $fdd4,x              ; $fae0: bd d4 fd
	and $b7                  ; $fae3: 25 b7
	bne $fb2b                ; $fae5: d0 44
loc_fae7:
	lda $99                  ; $fae7: a5 99
	eor #$02                 ; $fae9: 49 02 WSYNC
	sta $99                  ; $faeb: 85 99
	jmp $fb2b                ; $faed: 4c 2b fb

; === Block $faf0-$fb10 (Code) ===
loc_faf0:
	lda $b9                  ; $faf0: a5 b9
	bmi $faf6                ; $faf2: 30 02
loc_faf4:
	lsr                      ; $faf4: 4a
	lsr                      ; $faf5: 4a
loc_faf6:
	and #$03                 ; $faf6: 29 03 RSYNC
	sta $99                  ; $faf8: 85 99
	tax                      ; $fafa: aa
	lda $fdd4,x              ; $fafb: bd d4 fd
	and $b7                  ; $fafe: 25 b7
	bne $fb2b                ; $fb00: d0 29
loc_fb02:
	lda $99                  ; $fb02: a5 99
	eor #$02                 ; $fb04: 49 02 WSYNC
	sta $99                  ; $fb06: 85 99
	tax                      ; $fb08: aa
	lda $fdd4,x              ; $fb09: bd d4 fd
	and $b7                  ; $fb0c: 25 b7
	bne $fb2b                ; $fb0e: d0 1b
loc_fb10:
	jmp $fb35                ; $fb10: 4c 35 fb

; === Block $fb13-$fb2a (Code) ===
loc_fb13:
	jsr $fc22                ; $fb13: 20 22 fc
loc_fb16:
	ldy $b8                  ; $fb16: a4 b8
	ldx $99                  ; $fb18: a6 99
	lda $fdd4,x              ; $fb1a: bd d4 fd
	and $b7                  ; $fb1d: 25 b7
	bne $fb2b                ; $fb1f: d0 0a
loc_fb21:
	ldx $ba                  ; $fb21: a6 ba
	lda $fdd4,x              ; $fb23: bd d4 fd
	and $b7                  ; $fb26: 25 b7
	bne $fb35                ; $fb28: d0 0b
loc_fb2a:
	rts                      ; $fb2a: 60

; === Block $fb2b-$fb35 (Code) ===
loc_fb2b:
	lda $0081,y              ; $fb2b: b9 81 00
	and #$fc                 ; $fb2e: 29 fc
	ora $99                  ; $fb30: 05 99
	sta $0081,y              ; $fb32: 99 81 00

; === Block $fb35-$fb51 (Code) ===
loc_fb35:
	lda $0081,y              ; $fb35: b9 81 00
	and #$03                 ; $fb38: 29 03 RSYNC
	lsr                      ; $fb3a: 4a
	bcc $fb5d                ; $fb3b: 90 20
loc_fb3d:
	tax                      ; $fb3d: aa
	lda $ff83,x              ; $fb3e: bd 83 ff
	clc                      ; $fb41: 18
	adc $0086,y              ; $fb42: 79 86 00
	sta $0086,y              ; $fb45: 99 86 00
	cmp #$0c                 ; $fb48: c9 0c REFP1
	bcs $fb52                ; $fb4a: b0 06
loc_fb4c:
	lda #$ab                 ; $fb4c: a9 ab
	sta $0086,y              ; $fb4e: 99 86 00
	rts                      ; $fb51: 60

; === Block $fb52-$fb56 (Code) ===
loc_fb52:
	cmp #$ac                 ; $fb52: c9 ac
	bcs $fb57                ; $fb54: b0 01
loc_fb56:
	rts                      ; $fb56: 60

; === Block $fb57-$fb5c (Code) ===
loc_fb57:
	lda #$0c                 ; $fb57: a9 0c REFP1
	sta $0086,y              ; $fb59: 99 86 00
	rts                      ; $fb5c: 60

; === Block $fb5d-$fb61 (Code) ===
loc_fb5d:
	tax                      ; $fb5d: aa
	lda $ff84,x              ; $fb5e: bd 84 ff
	jmp $fba9                ; $fb61: 4c a9 fb

; === Block $fb64-$fb81 (Code) ===
loc_fb64:
	lda $008c,y              ; $fb64: b9 8c 00
	cmp #$42                 ; $fb67: c9 42 WSYNC
	bne $fba7                ; $fb69: d0 3c
loc_fb6b:
	lda $0081,y              ; $fb6b: b9 81 00
	and #$10                 ; $fb6e: 29 10 RESP0
	ora #$70                 ; $fb70: 09 70
	sta $b9                  ; $fb72: 85 b9
	lda $f7                  ; $fb74: a5 f7
	sbc $9e                  ; $fb76: e5 9e
	eor $8a                  ; $fb78: 45 8a
	and #$03                 ; $fb7a: 29 03 RSYNC
	ora $b9                  ; $fb7c: 05 b9
	sta $0081,y              ; $fb7e: 99 81 00
	jmp $fbbc                ; $fb81: 4c bc fb

; === Block $fb84-$fba3 (Code) ===
loc_fb84:
	lda $f6                  ; $fb84: a5 f6
	and #$78                 ; $fb86: 29 78
	sta $f6                  ; $fb88: 85 f6
	lda $0081,y              ; $fb8a: b9 81 00
	and #$08                 ; $fb8d: 29 08 COLUPF
	bne $fbf6                ; $fb8f: d0 65
loc_fb91:
	lda $008c,y              ; $fb91: b9 8c 00
	cmp #$42                 ; $fb94: c9 42 WSYNC
	beq $fbb1                ; $fb96: f0 19
loc_fb98:
	cmp #$50                 ; $fb98: c9 50 RESP0
	beq $fbc0                ; $fb9a: f0 24
loc_fb9c:
	lda $0086,y              ; $fb9c: b9 86 00
	cmp #$52                 ; $fb9f: c9 52 RESM0
	beq $fba7                ; $fba1: f0 04

; === Block $fba3-$fbb0 (Code) ===
loc_fba3:
	lda #$ff                 ; $fba3: a9 ff
	bmi $fba9                ; $fba5: 30 02
loc_fba7:
	lda #$01                 ; $fba7: a9 01 VBLANK
loc_fba9:
	clc                      ; $fba9: 18
	adc $008c,y              ; $fbaa: 79 8c 00
	sta $008c,y              ; $fbad: 99 8c 00
	rts                      ; $fbb0: 60

; === Block $fbb1-$fbbc (Code) ===
loc_fbb1:
	lda $0086,y              ; $fbb1: b9 86 00
	cmp #$58                 ; $fbb4: c9 58 AUDF1
	beq $fbd1                ; $fbb6: f0 19
loc_fbb8:
	cmp #$52                 ; $fbb8: c9 52 RESM0
	beq $fba7                ; $fbba: f0 eb

; === Block $fbbc-$fbd0 (Code) ===
loc_fbbc:
	lda #$ff                 ; $fbbc: a9 ff
	bmi $fbc9                ; $fbbe: 30 09
loc_fbc0:
	lda $0086,y              ; $fbc0: b9 86 00
	cmp #$5e                 ; $fbc3: c9 5e ENAM1
	beq $fba3                ; $fbc5: f0 dc
loc_fbc7:
	lda #$01                 ; $fbc7: a9 01 VBLANK
loc_fbc9:
	clc                      ; $fbc9: 18
	adc $0086,y              ; $fbca: 79 86 00
	sta $0086,y              ; $fbcd: 99 86 00
	rts                      ; $fbd0: 60

; === Block $fbd1-$fbf3 (Code) ===
loc_fbd1:
	lda $0081,y              ; $fbd1: b9 81 00
	and #$f0                 ; $fbd4: 29 f0
	sta $b9                  ; $fbd6: 85 b9
	lda $0081,y              ; $fbd8: b9 81 00
	and #$03                 ; $fbdb: 29 03 RSYNC
	clc                      ; $fbdd: 18
	adc #$ff                 ; $fbde: 69 ff
	cmp #$ff                 ; $fbe0: c9 ff
	beq $fbeb                ; $fbe2: f0 07
loc_fbe4:
	ora $b9                  ; $fbe4: 05 b9
	sta $0081,y              ; $fbe6: 99 81 00
	bne $fbbc                ; $fbe9: d0 d1
loc_fbeb:
	lda $0081,y              ; $fbeb: b9 81 00
	ora #$08                 ; $fbee: 09 08 COLUPF
	sta $0081,y              ; $fbf0: 99 81 00
	jmp $fba3                ; $fbf3: 4c a3 fb

; === Block $fbf6-$fc1f (Code) ===
loc_fbf6:
	lda $008c,y              ; $fbf6: b9 8c 00
	cmp #$32                 ; $fbf9: c9 32
	bne $fba3                ; $fbfb: d0 a6
loc_fbfd:
	lda $0081,y              ; $fbfd: b9 81 00
	and #$80                 ; $fc00: 29 80
	sta $b9                  ; $fc02: 85 b9
	lda $8a                  ; $fc04: a5 8a
	adc $90                  ; $fc06: 65 90
	eor $9e                  ; $fc08: 45 9e
	eor $f7                  ; $fc0a: 45 f7
	and #$03                 ; $fc0c: 29 03 RSYNC
	tax                      ; $fc0e: aa
	asl                      ; $fc0f: 0a
	asl                      ; $fc10: 0a
	asl                      ; $fc11: 0a
	asl                      ; $fc12: 0a
	ora $b9                  ; $fc13: 05 b9
	sta $b9                  ; $fc15: 85 b9
	lda $ff7f,x              ; $fc17: bd 7f ff
	ora $b9                  ; $fc1a: 05 b9
	sta $0081,y              ; $fc1c: 99 81 00
	jmp $fb35                ; $fc1f: 4c 35 fb

; === Block $fc22-$fc4f (Code) ===
loc_fc22:
	lda $0280                ; $fc22: ad 80 02 SWCHA
	lsr                      ; $fc25: 4a
	lsr                      ; $fc26: 4a
	lsr                      ; $fc27: 4a
	lsr                      ; $fc28: 4a
	eor #$0f                 ; $fc29: 49 0f PF2
	beq $fc69                ; $fc2b: f0 3c
loc_fc2d:
	cmp #$05                 ; $fc2d: c9 05 NUSIZ1
	bmi $fc5c                ; $fc2f: 30 2b
loc_fc31:
	cmp #$08                 ; $fc31: c9 08 COLUPF
	beq $fc5c                ; $fc33: f0 27
loc_fc35:
	sta $b9                  ; $fc35: 85 b9
	lda $85                  ; $fc37: a5 85
	and #$03                 ; $fc39: 29 03 RSYNC
	tay                      ; $fc3b: a8
	lda $fde1,y              ; $fc3c: b9 e1 fd
	tay                      ; $fc3f: a8
	and $b9                  ; $fc40: 25 b9
	bne $fc59                ; $fc42: d0 15
loc_fc44:
	tya                      ; $fc44: 98
	ora $b9                  ; $fc45: 05 b9
	eor #$03                 ; $fc47: 49 03 RSYNC
	beq $fc52                ; $fc49: f0 07
loc_fc4b:
	lda $b9                  ; $fc4b: a5 b9
	and #$0c                 ; $fc4d: 29 0c REFP1
	jmp $fc5c                ; $fc4f: 4c 5c fc

; === Block $fc52-$fc56 (Code) ===
loc_fc52:
	lda $b9                  ; $fc52: a5 b9
	and #$03                 ; $fc54: 29 03 RSYNC
	jmp $fc5c                ; $fc56: 4c 5c fc

; === Block $fc59-$fc5c (Code) ===
loc_fc59:
	tay                      ; $fc59: a8
	eor $b9                  ; $fc5a: 45 b9

; === Block $fc5c-$fc68 (Code) ===
loc_fc5c:
	tax                      ; $fc5c: aa
	lda $fdd8,x              ; $fc5d: bd d8 fd
	sta $99                  ; $fc60: 85 99
	lda $85                  ; $fc62: a5 85
	and #$03                 ; $fc64: 29 03 RSYNC
	sta $ba                  ; $fc66: 85 ba
	rts                      ; $fc68: 60

; === Block $fc69-$fc71 (Code) ===
loc_fc69:
	lda $85                  ; $fc69: a5 85
	and #$03                 ; $fc6b: 29 03 RSYNC
	sta $ba                  ; $fc6d: 85 ba
	sta $99                  ; $fc6f: 85 99
	rts                      ; $fc71: 60

; === Block $fc72-$fcb5 (Code) ===
loc_fc72:
	lsr                      ; $fc72: 4a
	lsr                      ; $fc73: 4a
	tay                      ; $fc74: a8
	lda $fde5,y              ; $fc75: b9 e5 fd
	sta $b7                  ; $fc78: 85 b7
	asl                      ; $fc7a: 0a
	adc $b7                  ; $fc7b: 65 b7
	asl                      ; $fc7d: 0a
	sta $b7                  ; $fc7e: 85 b7
	txa                      ; $fc80: 8a
	lsr                      ; $fc81: 4a
	lsr                      ; $fc82: 4a
	lsr                      ; $fc83: 4a
	ldx #$00                 ; $fc84: a2 00 VSYNC
	cmp #$0c                 ; $fc86: c9 0c REFP1
	bmi $fc91                ; $fc88: 30 07
loc_fc8a:
	eor #$ff                 ; $fc8a: 49 ff
	clc                      ; $fc8c: 18
	adc #$16                 ; $fc8d: 69 16 AUDC1
	ldx #$01                 ; $fc8f: a2 01 VBLANK
loc_fc91:
	lsr                      ; $fc91: 4a
	bcc $fcb6                ; $fc92: 90 22
loc_fc94:
	clc                      ; $fc94: 18
	adc $b7                  ; $fc95: 65 b7
	sta $b7                  ; $fc97: 85 b7
	lda $80                  ; $fc99: a5 80
	asl                      ; $fc9b: 0a
	tay                      ; $fc9c: a8
	lda $ff77,y              ; $fc9d: b9 77 ff
	clc                      ; $fca0: 18
	adc $b7                  ; $fca1: 65 b7
	sta $b9                  ; $fca3: 85 b9
	iny                      ; $fca5: c8
	lda $ff77,y              ; $fca6: b9 77 ff
	adc #$00                 ; $fca9: 69 00 VSYNC
	sta $ba                  ; $fcab: 85 ba
	ldy #$00                 ; $fcad: a0 00 VSYNC
	lda ($b9),y              ; $fcaf: b1 b9
	cpx #$00                 ; $fcb1: e0 00 VSYNC
	bne $fcdb                ; $fcb3: d0 26
loc_fcb5:
	rts                      ; $fcb5: 60

; === Block $fcb6-$fcda (Code) ===
loc_fcb6:
	adc $b7                  ; $fcb6: 65 b7
	sta $b7                  ; $fcb8: 85 b7
	lda $80                  ; $fcba: a5 80
	asl                      ; $fcbc: 0a
	tay                      ; $fcbd: a8
	lda $ff77,y              ; $fcbe: b9 77 ff
	clc                      ; $fcc1: 18
	adc $b7                  ; $fcc2: 65 b7
	sta $b9                  ; $fcc4: 85 b9
	iny                      ; $fcc6: c8
	lda $ff77,y              ; $fcc7: b9 77 ff
	adc #$00                 ; $fcca: 69 00 VSYNC
	sta $ba                  ; $fccc: 85 ba
	ldy #$00                 ; $fcce: a0 00 VSYNC
	lda ($b9),y              ; $fcd0: b1 b9
	lsr                      ; $fcd2: 4a
	lsr                      ; $fcd3: 4a
	lsr                      ; $fcd4: 4a
	lsr                      ; $fcd5: 4a
	cpx #$00                 ; $fcd6: e0 00 VSYNC
	bne $fcdb                ; $fcd8: d0 01
loc_fcda:
	rts                      ; $fcda: 60

; === Block $fcdb-$fcf3 (Code) ===
loc_fcdb:
	tax                      ; $fcdb: aa
	and #$0a                 ; $fcdc: 29 0a CTRLPF
	sta $b7                  ; $fcde: 85 b7
	txa                      ; $fce0: 8a
	and #$05                 ; $fce1: 29 05 NUSIZ1
	tax                      ; $fce3: aa
	and #$04                 ; $fce4: 29 04 NUSIZ0
	lsr                      ; $fce6: 4a
	lsr                      ; $fce7: 4a
	ora $b7                  ; $fce8: 05 b7
	sta $b7                  ; $fcea: 85 b7
	txa                      ; $fcec: 8a
	and #$01                 ; $fced: 29 01 VBLANK
	asl                      ; $fcef: 0a
	asl                      ; $fcf0: 0a
	ora $b7                  ; $fcf1: 05 b7
	rts                      ; $fcf3: 60

; === Block $fcf4-$fd03 (Code) ===
loc_fcf4:
	tax                      ; $fcf4: aa
	and #$03                 ; $fcf5: 29 03 RSYNC
	sta $b9                  ; $fcf7: 85 b9
	txa                      ; $fcf9: 8a
	lsr                      ; $fcfa: 4a
	lsr                      ; $fcfb: 4a
	tax                      ; $fcfc: aa
	lda $ff86,x              ; $fcfd: bd 86 ff
	clc                      ; $fd00: 18
	adc $b9                  ; $fd01: 65 b9
	rts                      ; $fd03: 60

; === Block $fd04-$fd15 (Code) ===
loc_fd04:
	ldx $80                  ; $fd04: a6 80
	lda $fd16,x              ; $fd06: bd 16 fd
	sta $08                  ; $fd09: 85 08 COLUPF
	ldx #$29                 ; $fd0b: a2 29 RESMP1
	lda $ffc5,x              ; $fd0d: bd c5 ff
	sta $bb,x                ; $fd10: 95 bb
	dex                      ; $fd12: ca
	bpl $fd0d                ; $fd13: 10 f8
loc_fd15:
	rts                      ; $fd15: 60

; === Block $ffee-$fffc (Code) ===
entry_ffee:
	*isc $6692,x             ; $ffee: ff 92 66
	ror $8d                  ; $fff1: 66 8d
	sed                      ; $fff3: f8
	*isc $924c,x             ; $fff4: ff 4c 92
	beq $fff8                ; $fff7: f0 ff BANK0
loc_fff9:
	bpl $1003b               ; $fff9: 10 40
loc_fffb:
	*jam                     ; $fffb: 52
	brk                      ; $fffc: 00

; === Block $fff2-$fff5 (Code) ===
loc_fff2:
	sta $fff8                ; $fff2: 8d f8 ff BANK0
	jmp $f092                ; $fff5: 4c 92 f0

; === Block $fff8-$fffb (Code) ===
loc_fff8:
	*isc $4010,x             ; $fff8: ff 10 40 RESP0

