; 🌺 Peony Disassembly → 🌸 Poppy Assembly
; Platform: Atari 2600
; Size: 4096 bytes

.cpu 6502

; === Code Block $f118-$f1ba ===
.org $f118

loc_f118:
	sta $8f                  ; $f118: 85 8f
	asl                      ; $f11a: 0a
	asl                      ; $f11b: 0a
	asl                      ; $f11c: 0a
	sta $8e                  ; $f11d: 85 8e
	lda $97                  ; $f11f: a5 97
	and #$18                 ; $f121: 29 18
	lsr                      ; $f123: 4a
	lsr                      ; $f124: 4a
	adc $8e                  ; $f125: 65 8e
	stx $8e                  ; $f127: 86 8e
	tax                      ; $f129: aa
	lda $8f                  ; $f12a: a5 8f
	bmi $f138                ; $f12c: 30 0a
loc_f12e:
	lda $f053,x              ; $f12e: bd 53 f0
	ldy $f054,x              ; $f131: bc 54 f0
	ldx #$00                 ; $f134: a2 00
	beq $f140                ; $f136: f0 08
loc_f138:
	lda $f093,x              ; $f138: bd 93 f0
	ldy $f094,x              ; $f13b: bc 94 f0
	ldx #$04                 ; $f13e: a2 04
loc_f140:
	sta $8f                  ; $f140: 85 8f
	sty $90                  ; $f142: 84 90
	stx $9a                  ; $f144: 86 9a
	lda $97                  ; $f146: a5 97
	and #$07                 ; $f148: 29 07
	tay                      ; $f14a: a8
	lda ($8f),y              ; $f14b: b1 8f
	eor #$ff                 ; $f14d: 49 ff
	beq $f185                ; $f14f: f0 34
loc_f151:
	eor #$ff                 ; $f151: 49 ff
	tax                      ; $f153: aa
	and #$1f                 ; $f154: 29 1f
	pha                      ; $f156: 48
	txa                      ; $f157: 8a
	lsr                      ; $f158: 4a
	lsr                      ; $f159: 4a
	lsr                      ; $f15a: 4a
	lsr                      ; $f15b: 4a
	lsr                      ; $f15c: 4a
	tax                      ; $f15d: aa
	lda $9a                  ; $f15e: a5 9a
	clc                      ; $f160: 18
	adc $f00f,x              ; $f161: 7d 0f f0
	sta $9a                  ; $f164: 85 9a
	lda $f017,x              ; $f166: bd 17 f0
	ldx $8e                  ; $f169: a6 8e
	sta $95,x                ; $f16b: 95 95
	sta $15,x                ; $f16d: 95 15
	pla                      ; $f16f: 68
	sta $17,x                ; $f170: 95 17
	sta $91,x                ; $f172: 95 91
	tya                      ; $f174: 98
	tax                      ; $f175: aa
	ldy #$08                 ; $f176: a0 08
	lda ($8f),y              ; $f178: b1 8f
	and $f110,x              ; $f17a: 3d 10 f1
	beq $f181                ; $f17d: f0 02
loc_f17f:
	lda #$0f                 ; $f17f: a9 0f
loc_f181:
	ora #$0d                 ; $f181: 09 0d
	sbc $9a                  ; $f183: e5 9a
loc_f185:
	ldy $8e                  ; $f185: a4 8e
	sta $0019,y              ; $f187: 99 19 00
	sta $0093,y              ; $f18a: 99 93 00
	ldy $8e                  ; $f18d: a4 8e
	beq $f1ba                ; $f18f: f0 29
loc_f191:
	lda $99                  ; $f191: a5 99
	cmp #$00                 ; $f193: c9 00
	bmi $f1ba                ; $f195: 30 23
loc_f197:
	lda $97                  ; $f197: a5 97
	and #$07                 ; $f199: 29 07
	tax                      ; $f19b: aa
	lda $97                  ; $f19c: a5 97
	lsr                      ; $f19e: 4a
	lsr                      ; $f19f: 4a
	lsr                      ; $f1a0: 4a
	tay                      ; $f1a1: a8
	lda $f01f,y              ; $f1a2: b9 1f f0
	and $f110,x              ; $f1a5: 3d 10 f1
	beq $f1ba                ; $f1a8: f0 10
loc_f1aa:
	lda $98                  ; $f1aa: a5 98
	bne $f1ba                ; $f1ac: d0 0c
loc_f1ae:
	lda #$00                 ; $f1ae: a9 00
	sta $18                  ; $f1b0: 85 18
	lda #$08                 ; $f1b2: a9 08
	sta $16                  ; $f1b4: 85 16
	lda #$02                 ; $f1b6: a9 02
	sta $1a                  ; $f1b8: 85 1a
loc_f1ba:
	rts                      ; $f1ba: 60

; === Code Block $f1bb-$f1ef ===
.org $f1bb

loc_f1bb:
	inc $98                  ; $f1bb: e6 98
	lda $98                  ; $f1bd: a5 98
	eor #$04                 ; $f1bf: 49 04
	bne $f1d1                ; $f1c1: d0 0e
loc_f1c3:
	sta $98                  ; $f1c3: 85 98
	inc $97                  ; $f1c5: e6 97
	lda $97                  ; $f1c7: a5 97
	eor #$20                 ; $f1c9: 49 20
	bne $f1d1                ; $f1cb: d0 04
loc_f1cd:
	sta $97                  ; $f1cd: 85 97
	inc $99                  ; $f1cf: e6 99
loc_f1d1:
	ldx #$00                 ; $f1d1: a2 00
	stx $93                  ; $f1d3: 86 93
	stx $94                  ; $f1d5: 86 94
	ldy $99                  ; $f1d7: a4 99
	lda $f023,y              ; $f1d9: b9 23 f0
	cmp #$ff                 ; $f1dc: c9 ff
	bne $f1e5                ; $f1de: d0 05
loc_f1e0:
	stx $99                  ; $f1e0: 86 99
	lda $f023,x              ; $f1e2: bd 23 f0
loc_f1e5:
	jsr $f118                ; $f1e5: 20 18 f1
loc_f1e8:
	ldy $99                  ; $f1e8: a4 99
	lda $f03b,y              ; $f1ea: b9 3b f0
	ldx #$01                 ; $f1ed: a2 01
	jmp $f118                ; $f1ef: 4c 18 f1

; === Code Block $f305-$f356 ===
.org $f305

entry_f305:
	sei                      ; $f305: 78
	cld                      ; $f306: d8
	ldx #$ff                 ; $f307: a2 ff
	txs                      ; $f309: 9a
	lda #$00                 ; $f30a: a9 00
	sta $00,x                ; $f30c: 95 00
	dex                      ; $f30e: ca
	bne $f30c                ; $f30f: d0 fb
loc_f311:
	inc $81                  ; $f311: e6 81
	lda $ce                  ; $f313: a5 ce
	cmp #$01                 ; $f315: c9 01
	beq $f332                ; $f317: f0 19
loc_f319:
	inc $8c                  ; $f319: e6 8c
	lda $8c                  ; $f31b: a5 8c
	cmp #$cd                 ; $f31d: c9 cd
	bne $f33c                ; $f31f: d0 1b
loc_f321:
	inc $8d                  ; $f321: e6 8d
	lda #$00                 ; $f323: a9 00
	sta $8c                  ; $f325: 85 8c
	ldx $80                  ; $f327: a6 80
	lda $f2f2,x              ; $f329: bd f2 f2
	cmp $8d                  ; $f32c: c5 8d
	bne $f33c                ; $f32e: d0 0c
loc_f330:
	inc $80                  ; $f330: e6 80
loc_f332:
	lda #$00                 ; $f332: a9 00
	sta $ce                  ; $f334: 85 ce
	sta $8c                  ; $f336: 85 8c
	sta $8d                  ; $f338: 85 8d
	sta $85                  ; $f33a: 85 85
loc_f33c:
	ldx $80                  ; $f33c: a6 80
	lda $f2f2,x              ; $f33e: bd f2 f2
	cmp #$ff                 ; $f341: c9 ff
	beq $f359                ; $f343: f0 14
loc_f345:
	jsr $fbfa                ; $f345: 20 fa fb
loc_f348:
	jsr $fc16                ; $f348: 20 16 fc
loc_f34b:
	lda $0284                ; $f34b: ad 84 02
	bne $f34b                ; $f34e: d0 fb
loc_f350:
	sta $02                  ; $f350: 85 02
	lda #$80                 ; $f352: a9 80
	sta $01                  ; $f354: 85 01
	jmp $f363                ; $f356: 4c 63 f3

; === Code Block $f359-$f360 ===
.org $f359

loc_f359:
	jsr $f3e2                ; $f359: 20 e2 f3
loc_f35c:
	lda #$00                 ; $f35c: a9 00
	sta $97                  ; $f35e: 85 97
	jmp $f3df                ; $f360: 4c df f3

; === Code Block $f363-$f395 ===
.org $f363

loc_f363:
	lda $80                  ; $f363: a5 80
	cmp #$01                 ; $f365: c9 01
	beq $f39e                ; $f367: f0 35
loc_f369:
	cmp #$03                 ; $f369: c9 03
	beq $f3b6                ; $f36b: f0 49
loc_f36d:
	cmp #$04                 ; $f36d: c9 04
	beq $f3aa                ; $f36f: f0 39
loc_f371:
	cmp #$06                 ; $f371: c9 06
	beq $f3bc                ; $f373: f0 47
loc_f375:
	cmp #$07                 ; $f375: c9 07
	beq $f3a4                ; $f377: f0 2b
loc_f379:
	cmp #$08                 ; $f379: c9 08
	beq $f3bc                ; $f37b: f0 3f
loc_f37d:
	cmp #$09                 ; $f37d: c9 09
	beq $f3aa                ; $f37f: f0 29
loc_f381:
	cmp #$0b                 ; $f381: c9 0b
	beq $f398                ; $f383: f0 13
loc_f385:
	cmp #$0c                 ; $f385: c9 0c
	beq $f3b0                ; $f387: f0 27
loc_f389:
	cmp #$0e                 ; $f389: c9 0e
	beq $f3c8                ; $f38b: f0 3b
loc_f38d:
	cmp #$0f                 ; $f38d: c9 0f
	beq $f3bc                ; $f38f: f0 2b
loc_f391:
	cmp #$11                 ; $f391: c9 11
	beq $f3c2                ; $f393: f0 2d
loc_f395:
	jmp $f3ce                ; $f395: 4c ce f3

; === Code Block $f398-$f39b ===
.org $f398

loc_f398:
	jsr $f541                ; $f398: 20 41 f5
loc_f39b:
	jmp $f3ce                ; $f39b: 4c ce f3

; === Code Block $f39e-$f3a1 ===
.org $f39e

loc_f39e:
	jsr $f6f7                ; $f39e: 20 f7 f6
loc_f3a1:
	jmp $f3ce                ; $f3a1: 4c ce f3

; === Code Block $f3a4-$f3a7 ===
.org $f3a4

loc_f3a4:
	jsr $f65b                ; $f3a4: 20 5b f6
loc_f3a7:
	jmp $f3ce                ; $f3a7: 4c ce f3

; === Code Block $f3aa-$f3ad ===
.org $f3aa

loc_f3aa:
	jsr $f8ca                ; $f3aa: 20 ca f8
loc_f3ad:
	jmp $f3ce                ; $f3ad: 4c ce f3

; === Code Block $f3b0-$f3b3 ===
.org $f3b0

loc_f3b0:
	jsr $f3ea                ; $f3b0: 20 ea f3
loc_f3b3:
	jmp $f3ce                ; $f3b3: 4c ce f3

; === Code Block $f3b6-$f3b9 ===
.org $f3b6

loc_f3b6:
	jsr $f5e5                ; $f3b6: 20 e5 f5
loc_f3b9:
	jmp $f3ce                ; $f3b9: 4c ce f3

; === Code Block $f3bc-$f3bf ===
.org $f3bc

loc_f3bc:
	jsr $f7e7                ; $f3bc: 20 e7 f7
loc_f3bf:
	jmp $f3ce                ; $f3bf: 4c ce f3

; === Code Block $f3c2-$f3c5 ===
.org $f3c2

loc_f3c2:
	jsr $f77b                ; $f3c2: 20 7b f7
loc_f3c5:
	jmp $f3ce                ; $f3c5: 4c ce f3

; === Code Block $f3c8-$f3cb ===
.org $f3c8

loc_f3c8:
	jsr $f473                ; $f3c8: 20 73 f4
loc_f3cb:
	jmp $f3ce                ; $f3cb: 4c ce f3

; === Code Block $f3ce-$f3df ===
.org $f3ce

loc_f3ce:
	ldx #$03                 ; $f3ce: a2 03
	sta $02                  ; $f3d0: 85 02
	dex                      ; $f3d2: ca
	bne $f3d0                ; $f3d3: d0 fb
loc_f3d5:
	sta $02                  ; $f3d5: 85 02
	lda #$82                 ; $f3d7: a9 82
	sta $01                  ; $f3d9: 85 01
	lda #$02                 ; $f3db: a9 02
	sta $01                  ; $f3dd: 85 01

; === Code Block $f3df-$f3df ===
.org $f3df

loc_f3df:
	jmp $f311                ; $f3df: 4c 11 f3

; === Code Block $f3e2-$f3e9 ===
.org $f3e2

loc_f3e2:
	lda #$80                 ; $f3e2: a9 80
	sta $01                  ; $f3e4: 85 01
	jsr $fa0b                ; $f3e6: 20 0b fa
loc_f3e9:
	rts                      ; $f3e9: 60

; === Code Block $f3ea-$f44d ===
.org $f3ea

loc_f3ea:
	lda #$71                 ; $f3ea: a9 71
	sta $0a                  ; $f3ec: 85 0a
	lda #$00                 ; $f3ee: a9 00
	sta $08                  ; $f3f0: 85 08
	lda #$17                 ; $f3f2: a9 17
	sta $04                  ; $f3f4: 85 04
	sta $05                  ; $f3f6: 85 05
	lda #$5c                 ; $f3f8: a9 5c
	sta $02                  ; $f3fa: 85 02
	ldx #$00                 ; $f3fc: a2 00
	jsr $fc40                ; $f3fe: 20 40 fc
loc_f401:
	sta $20                  ; $f401: 85 20
	lda #$2e                 ; $f403: a9 2e
	ldx #$01                 ; $f405: a2 01
	jsr $fc40                ; $f407: 20 40 fc
loc_f40a:
	sty $21,x                ; $f40a: 94 21
	sta $02                  ; $f40c: 85 02
	lda #$0b                 ; $f40e: a9 0b
	sbc $92                  ; $f410: e5 92
	and #$07                 ; $f412: 29 07
	adc #$01                 ; $f414: 69 01
	sta $09                  ; $f416: 85 09
	lda #$78                 ; $f418: a9 78
	sta $0d                  ; $f41a: 85 0d
	ldx #$e3                 ; $f41c: a2 e3
	lda #$00                 ; $f41e: a9 00
	sta $08                  ; $f420: 85 08
	lda #$58                 ; $f422: a9 58
	sta $07                  ; $f424: 85 07
	lda #$57                 ; $f426: a9 57
	sta $06                  ; $f428: 85 06
	txa                      ; $f42a: 8a
	sbc $f20d,x              ; $f42b: fd 0d f2
	sbc $81                  ; $f42e: e5 81
	rol                      ; $f430: 2a
	and #$1f                 ; $f431: 29 1f
	tay                      ; $f433: a8
	lda $fde1,y              ; $f434: b9 e1 fd
	sta $1b                  ; $f437: 85 1b
	sta $1c                  ; $f439: 85 1c
	cpx #$40                 ; $f43b: e0 40
	bcc $f450                ; $f43d: 90 11
loc_f43f:
	lda #$07                 ; $f43f: a9 07
	sbc $f1de,x              ; $f441: fd de f1
	sta $20                  ; $f444: 85 20
	lda #$07                 ; $f446: a9 07
	adc $f1de,x              ; $f448: 7d de f1
	sta $21                  ; $f44b: 85 21
	jmp $f45e                ; $f44d: 4c 5e f4

; === Code Block $f450-$f472 ===
.org $f450

loc_f450:
	lda #$0f                 ; $f450: a9 0f
	sbc $f210,x              ; $f452: fd 10 f2
	sta $20                  ; $f455: 85 20
	lda #$00                 ; $f457: a9 00
	adc $f210,x              ; $f459: 7d 10 f2
	sta $21                  ; $f45c: 85 21
loc_f45e:
	sta $02                  ; $f45e: 85 02
	sta $2a                  ; $f460: 85 2a
	dex                      ; $f462: ca
	bne $f42a                ; $f463: d0 c5
loc_f465:
	sta $02                  ; $f465: 85 02
	lda #$00                 ; $f467: a9 00
	sta $09                  ; $f469: 85 09
	sta $1b                  ; $f46b: 85 1b
	sta $1c                  ; $f46d: 85 1c
	jsr $fc0c                ; $f46f: 20 0c fc
loc_f472:
	rts                      ; $f472: 60

; === Code Block $f473-$f4fa ===
.org $f473

loc_f473:
	lda #$01                 ; $f473: a9 01
	sta $0a                  ; $f475: 85 0a
	ldx #$00                 ; $f477: a2 00
	lda #$58                 ; $f479: a9 58
	jsr $fc40                ; $f47b: 20 40 fc
loc_f47e:
	lda #$07                 ; $f47e: a9 07
	sta $04                  ; $f480: 85 04
	sta $20,x                ; $f482: 95 20
	lda #$00                 ; $f484: a9 00
	sta $09                  ; $f486: 85 09
	sta $06                  ; $f488: 85 06
	sta $02                  ; $f48a: 85 02
	inc $a2                  ; $f48c: e6 a2
	lda $a0                  ; $f48e: a5 a0
	cmp #$07                 ; $f490: c9 07
	bne $f49a                ; $f492: d0 06
loc_f494:
	inc $a1                  ; $f494: e6 a1
	lda #$01                 ; $f496: a9 01
	sta $a0                  ; $f498: 85 a0
loc_f49a:
	lda $a0                  ; $f49a: a5 a0
	sta $9d                  ; $f49c: 85 9d
	ldy $a1                  ; $f49e: a4 a1
	sty $9c                  ; $f4a0: 84 9c
	lda $fef2,y              ; $f4a2: b9 f2 fe
	sta $9e                  ; $f4a5: 85 9e
	lda #$38                 ; $f4a7: a9 38
	sta $9f                  ; $f4a9: 85 9f
	lda #$00                 ; $f4ab: a9 00
	sta $1b                  ; $f4ad: 85 1b
	sta $1c                  ; $f4af: 85 1c
	sta $09                  ; $f4b1: 85 09
	lda #$00                 ; $f4b3: a9 00
	sta $08                  ; $f4b5: 85 08
	txa                      ; $f4b7: 8a
	adc $81                  ; $f4b8: 65 81
	tax                      ; $f4ba: aa
	inx                      ; $f4bb: e8
	txa                      ; $f4bc: 8a
	adc $f1f2,x              ; $f4bd: 7d f2 f1
	sbc $81                  ; $f4c0: e5 81
	asl                      ; $f4c2: 0a
	adc $f1f2,x              ; $f4c3: 7d f2 f1
	and $7f                  ; $f4c6: 25 7f
	tay                      ; $f4c8: a8
	lda $fcbe,y              ; $f4c9: b9 be fc
	sta $0e                  ; $f4cc: 85 0e
	lda #$ff                 ; $f4ce: a9 ff
	sta $0d                  ; $f4d0: 85 0d
	txa                      ; $f4d2: 8a
	sbc $81                  ; $f4d3: e5 81
	and $1f                  ; $f4d5: 25 1f
	sta $20                  ; $f4d7: 85 20
	sta $02                  ; $f4d9: 85 02
	sta $02                  ; $f4db: 85 02
	lda #$00                 ; $f4dd: a9 00
	sta $0e                  ; $f4df: 85 0e
	lda #$02                 ; $f4e1: a9 02
	sta $09                  ; $f4e3: 85 09
	lda $9d                  ; $f4e5: a5 9d
	cmp #$07                 ; $f4e7: c9 07
	bne $f4fd                ; $f4e9: d0 12
loc_f4eb:
	inc $9c                  ; $f4eb: e6 9c
	ldy $9c                  ; $f4ed: a4 9c
	lda $fef2,y              ; $f4ef: b9 f2 fe
	sta $9e                  ; $f4f2: 85 9e
	lda #$00                 ; $f4f4: a9 00
	sta $9d                  ; $f4f6: 85 9d
	sta $1b                  ; $f4f8: 85 1b
	jmp $f505                ; $f4fa: 4c 05 f5

; === Code Block $f4fd-$f518 ===
.org $f4fd

loc_f4fd:
	adc $9e                  ; $f4fd: 65 9e
	tay                      ; $f4ff: a8
	lda $fe4f,y              ; $f500: b9 4f fe
	sta $1b                  ; $f503: 85 1b
loc_f505:
	tya                      ; $f505: 98
	cmp #$dc                 ; $f506: c9 dc
	beq $f51b                ; $f508: f0 11
loc_f50a:
	inc $9d                  ; $f50a: e6 9d
	sta $02                  ; $f50c: 85 02
	sta $02                  ; $f50e: 85 02
	sta $2a                  ; $f510: 85 2a
	dec $9f                  ; $f512: c6 9f
	bne $f4bb                ; $f514: d0 a5
loc_f516:
	inc $a0                  ; $f516: e6 a0
	jmp $f52c                ; $f518: 4c 2c f5

; === Code Block $f51b-$f529 ===
.org $f51b

loc_f51b:
	ldx #$08                 ; $f51b: a2 08
	sta $02                  ; $f51d: 85 02
	sta $02                  ; $f51f: 85 02
	sta $2a                  ; $f521: 85 2a
	lda #$00                 ; $f523: a9 00
	sta $09                  ; $f525: 85 09
	sta $1b                  ; $f527: 85 1b
	jmp $f538                ; $f529: 4c 38 f5

; === Code Block $f52c-$f540 ===
.org $f52c

loc_f52c:
	sta $02                  ; $f52c: 85 02
	lda #$00                 ; $f52e: a9 00
	sta $09                  ; $f530: 85 09
	sta $1b                  ; $f532: 85 1b
	sta $08                  ; $f534: 85 08
	ldx #$07                 ; $f536: a2 07
loc_f538:
	sta $02                  ; $f538: 85 02
	dex                      ; $f53a: ca
	bne $f538                ; $f53b: d0 fb
loc_f53d:
	jsr $fc0c                ; $f53d: 20 0c fc
loc_f540:
	rts                      ; $f540: 60

; === Code Block $f541-$f572 ===
.org $f541

loc_f541:
	lda $cf                  ; $f541: a5 cf
	cmp #$01                 ; $f543: c9 01
	beq $f553                ; $f545: f0 0c
loc_f547:
	lda #$08                 ; $f547: a9 08
	sta $99                  ; $f549: 85 99
	lda #$00                 ; $f54b: a9 00
	sta $97                  ; $f54d: 85 97
	lda #$01                 ; $f54f: a9 01
	sta $cf                  ; $f551: 85 cf
loc_f553:
	lda #$01                 ; $f553: a9 01
	sta $0a                  ; $f555: 85 0a
	sta $02                  ; $f557: 85 02
	lda #$00                 ; $f559: a9 00
	ldx #$22                 ; $f55b: a2 22
	ldy #$00                 ; $f55d: a0 00
	lda #$0e                 ; $f55f: a9 0e
	sbc $92                  ; $f561: e5 92
	sta $06                  ; $f563: 85 06
	sta $08                  ; $f565: 85 08
	cpx #$0a                 ; $f567: e0 0a
	bcs $f575                ; $f569: b0 0a
loc_f56b:
	iny                      ; $f56b: c8
	lda #$02                 ; $f56c: a9 02
	and $92                  ; $f56e: 25 92
	sta $09                  ; $f570: 85 09
	jmp $f579                ; $f572: 4c 79 f5

; === Code Block $f575-$f592 ===
.org $f575

loc_f575:
	lda #$00                 ; $f575: a9 00
	sta $09                  ; $f577: 85 09
loc_f579:
	sta $02                  ; $f579: 85 02
	dex                      ; $f57b: ca
	bne $f55f                ; $f57c: d0 e1
loc_f57e:
	ldx #$3c                 ; $f57e: a2 3c
	tya                      ; $f580: 98
	sbc $f1f2,y              ; $f581: f9 f2 f1
	sbc $f1f2,x              ; $f584: fd f2 f1
	adc $81                  ; $f587: 65 81
	asl                      ; $f589: 0a
	rol                      ; $f58a: 2a
	tay                      ; $f58b: a8
	lda #$00                 ; $f58c: a9 00
	cpy #$78                 ; $f58e: c0 78
	bcc $f595                ; $f590: 90 03
loc_f592:
	jmp $f5ac                ; $f592: 4c ac f5

; === Code Block $f595-$f5a9 ===
.org $f595

loc_f595:
	cpx #$0b                 ; $f595: e0 0b
	bcc $f5ac                ; $f597: 90 13
loc_f599:
	lda $fce1,x              ; $f599: bd e1 fc
	and $fc78,y              ; $f59c: 39 78 fc
	sta $0f                  ; $f59f: 85 0f
	lda $fd12,x              ; $f5a1: bd 12 fd
	and $fc78,y              ; $f5a4: 39 78 fc
	sta $0e                  ; $f5a7: 85 0e
	jmp $f5b2                ; $f5a9: 4c b2 f5

; === Code Block $f5ac-$f5e4 ===
.org $f5ac

loc_f5ac:
	sta $0d                  ; $f5ac: 85 0d
	sta $0e                  ; $f5ae: 85 0e
	sta $0f                  ; $f5b0: 85 0f
loc_f5b2:
	sta $02                  ; $f5b2: 85 02
	txa                      ; $f5b4: 8a
	lda $f233,x              ; $f5b5: bd 33 f2
	sbc $81                  ; $f5b8: e5 81
	tay                      ; $f5ba: a8
	ror                      ; $f5bb: 6a
	and #$07                 ; $f5bc: 29 07
	adc #$57                 ; $f5be: 69 57
	sta $08                  ; $f5c0: 85 08
	sta $02                  ; $f5c2: 85 02
	sta $02                  ; $f5c4: 85 02
	dex                      ; $f5c6: ca
	bne $f580                ; $f5c7: d0 b7
loc_f5c9:
	lda #$00                 ; $f5c9: a9 00
	ldy #$09                 ; $f5cb: a0 09
	sta $1b                  ; $f5cd: 85 1b
	sta $1c                  ; $f5cf: 85 1c
	sta $09                  ; $f5d1: 85 09
	sta $08                  ; $f5d3: 85 08
	sta $02                  ; $f5d5: 85 02
	dey                      ; $f5d7: 88
	bne $f5cd                ; $f5d8: d0 f3
loc_f5da:
	ldy #$0c                 ; $f5da: a0 0c
	sta $02                  ; $f5dc: 85 02
	dey                      ; $f5de: 88
	bne $f5dc                ; $f5df: d0 fb
loc_f5e1:
	jsr $fc0c                ; $f5e1: 20 0c fc
loc_f5e4:
	rts                      ; $f5e4: 60

; === Code Block $f5e5-$f612 ===
.org $f5e5

loc_f5e5:
	lda #$71                 ; $f5e5: a9 71
	sta $0a                  ; $f5e7: 85 0a
	lda $07                  ; $f5e9: a5 07
	sta $04                  ; $f5eb: 85 04
	lda #$90                 ; $f5ed: a9 90
	ldx #$00                 ; $f5ef: a2 00
	jsr $fc40                ; $f5f1: 20 40 fc
loc_f5f4:
	sta $20,x                ; $f5f4: 95 20
	sta $12                  ; $f5f6: 85 12
	sta $14                  ; $f5f8: 85 14
	lda #$00                 ; $f5fa: a9 00
	sta $06                  ; $f5fc: 85 06
	sta $07                  ; $f5fe: 85 07
	lda #$02                 ; $f600: a9 02
	sta $1f                  ; $f602: 85 1f
	lda #$3f                 ; $f604: a9 3f
	sta $0d                  ; $f606: 85 0d
	sta $02                  ; $f608: 85 02
	ldx #$6f                 ; $f60a: a2 6f
	cpx #$6a                 ; $f60c: e0 6a
	bcc $f615                ; $f60e: 90 05
loc_f610:
	sta $02                  ; $f610: 85 02
	jmp $f61f                ; $f612: 4c 1f f6

; === Code Block $f615-$f65a ===
.org $f615

loc_f615:
	lda #$00                 ; $f615: a9 00
	sta $0e                  ; $f617: 85 0e
	sta $0f                  ; $f619: 85 0f
	lda #$06                 ; $f61b: a9 06
	sta $09                  ; $f61d: 85 09
loc_f61f:
	txa                      ; $f61f: 8a
	sbc $f276,x              ; $f620: fd 76 f2
	adc $81                  ; $f623: 65 81
	ror                      ; $f625: 6a
	asl                      ; $f626: 0a
	rol                      ; $f627: 2a
	tay                      ; $f628: a8
	lda $f1f2,y              ; $f629: b9 f2 f1
	asl                      ; $f62c: 0a
	adc $f1f2,y              ; $f62d: 79 f2 f1
	rol                      ; $f630: 2a
	clc                      ; $f631: 18
	clc                      ; $f632: 18
	sta $20                  ; $f633: 85 20
	sta $24                  ; $f635: 85 24
	sbc $f1f2                ; $f637: ed f2 f1
	sbc $84                  ; $f63a: e5 84
	asl                      ; $f63c: 0a
	tay                      ; $f63d: a8
	txa                      ; $f63e: 8a
	ora #$f7                 ; $f63f: 09 f7
	sta $1b                  ; $f641: 85 1b
	sta $02                  ; $f643: 85 02
	sta $2a                  ; $f645: 85 2a
	dex                      ; $f647: ca
	bne $f60c                ; $f648: d0 c2
loc_f64a:
	jsr $fc0c                ; $f64a: 20 0c fc
loc_f64d:
	sta $02                  ; $f64d: 85 02
	lda #$00                 ; $f64f: a9 00
	sta $09                  ; $f651: 85 09
	ldx $0a                  ; $f653: a6 0a
	dex                      ; $f655: ca
	sta $02                  ; $f656: 85 02
	bne $f655                ; $f658: d0 fb
loc_f65a:
	rts                      ; $f65a: 60

; === Code Block $f65b-$f684 ===
.org $f65b

loc_f65b:
	lda #$71                 ; $f65b: a9 71
	sta $0a                  ; $f65d: 85 0a
	lda $07                  ; $f65f: a5 07
	sta $04                  ; $f661: 85 04
	ldx #$00                 ; $f663: a2 00
	lda #$23                 ; $f665: a9 23
	jsr $fc40                ; $f667: 20 40 fc
loc_f66a:
	sta $20,x                ; $f66a: 95 20
	stx $1c                  ; $f66c: 86 1c
	sta $14                  ; $f66e: 85 14
	lda #$02                 ; $f670: a9 02
	sta $1f                  ; $f672: 85 1f
	lda #$3f                 ; $f674: a9 3f
	sta $0d                  ; $f676: 85 0d
	inc $84                  ; $f678: e6 84
	ldx #$74                 ; $f67a: a2 74
	cpx #$6e                 ; $f67c: e0 6e
	bcc $f687                ; $f67e: 90 07
loc_f680:
	cpx #$1e                 ; $f680: e0 1e
	bcc $f687                ; $f682: 90 03
loc_f684:
	jmp $f68f                ; $f684: 4c 8f f6

; === Code Block $f687-$f6f6 ===
.org $f687

loc_f687:
	lda #$0f                 ; $f687: a9 0f
	sbc $92                  ; $f689: e5 92
	and #$0f                 ; $f68b: 29 0f
	sta $09                  ; $f68d: 85 09
loc_f68f:
	tya                      ; $f68f: 98
	ora #$3f                 ; $f690: 09 3f
	sta $06                  ; $f692: 85 06
	txa                      ; $f694: 8a
	sbc $f276,x              ; $f695: fd 76 f2
	adc $81                  ; $f698: 65 81
	ror                      ; $f69a: 6a
	asl                      ; $f69b: 0a
	rol                      ; $f69c: 2a
	clc                      ; $f69d: 18
	tay                      ; $f69e: a8
	lda $f1f2,y              ; $f69f: b9 f2 f1
	sta $20                  ; $f6a2: 85 20
	lda $f1f2,y              ; $f6a4: b9 f2 f1
	adc $91                  ; $f6a7: 65 91
	asl                      ; $f6a9: 0a
	adc $92                  ; $f6aa: 65 92
	asl                      ; $f6ac: 0a
	sta $24                  ; $f6ad: 85 24
	lda #$00                 ; $f6af: a9 00
	cpx #$47                 ; $f6b1: e0 47
	bcs $f6c2                ; $f6b3: b0 0d
loc_f6b5:
	lda #$00                 ; $f6b5: a9 00
	cpx #$24                 ; $f6b7: e0 24
	bcc $f6c2                ; $f6b9: 90 07
loc_f6bb:
	lda #$0c                 ; $f6bb: a9 0c
	sta $09                  ; $f6bd: 85 09
	lda $fc30,x              ; $f6bf: bd 30 fc
loc_f6c2:
	sta $1b                  ; $f6c2: 85 1b
	lda #$02                 ; $f6c4: a9 02
	cpx #$48                 ; $f6c6: e0 48
	bcs $f6d2                ; $f6c8: b0 08
loc_f6ca:
	lda #$00                 ; $f6ca: a9 00
	cpx #$23                 ; $f6cc: e0 23
	bcs $f6d2                ; $f6ce: b0 02
loc_f6d0:
	lda #$02                 ; $f6d0: a9 02
loc_f6d2:
	sta $1f                  ; $f6d2: 85 1f
	sta $02                  ; $f6d4: 85 02
	sta $2a                  ; $f6d6: 85 2a
	dex                      ; $f6d8: ca
	bne $f67c                ; $f6d9: d0 a1
loc_f6db:
	jsr $fc0c                ; $f6db: 20 0c fc
loc_f6de:
	nop                      ; $f6de: ea
	nop                      ; $f6df: ea
	nop                      ; $f6e0: ea
	nop                      ; $f6e1: ea
	nop                      ; $f6e2: ea
	nop                      ; $f6e3: ea
	nop                      ; $f6e4: ea
	nop                      ; $f6e5: ea
	nop                      ; $f6e6: ea
	nop                      ; $f6e7: ea
	nop                      ; $f6e8: ea
	nop                      ; $f6e9: ea
	nop                      ; $f6ea: ea
	nop                      ; $f6eb: ea
	nop                      ; $f6ec: ea
	nop                      ; $f6ed: ea
	nop                      ; $f6ee: ea
	nop                      ; $f6ef: ea
	lda #$00                 ; $f6f0: a9 00
	sta $09                  ; $f6f2: 85 09
	sta $02                  ; $f6f4: 85 02
	rts                      ; $f6f6: 60

; === Code Block $f6f7-$f77a ===
.org $f6f7

loc_f6f7:
	lda #$00                 ; $f6f7: a9 00
	sta $0a                  ; $f6f9: 85 0a
	sta $07                  ; $f6fb: 85 07
	lda #$6f                 ; $f6fd: a9 6f
	sta $05                  ; $f6ff: 85 05
	ldx #$01                 ; $f701: a2 01
	lda $ca                  ; $f703: a5 ca
	jsr $fc40                ; $f705: 20 40 fc
loc_f708:
	sta $02                  ; $f708: 85 02
	lda #$00                 ; $f70a: a9 00
	ldx #$5c                 ; $f70c: a2 5c
	ldy #$00                 ; $f70e: a0 00
	lda #$00                 ; $f710: a9 00
	cpx #$46                 ; $f712: e0 46
	bcs $f71a                ; $f714: b0 04
loc_f716:
	adc $92                  ; $f716: 65 92
	and $05                  ; $f718: 25 05
loc_f71a:
	sta $09                  ; $f71a: 85 09
	sta $07                  ; $f71c: 85 07
	sta $02                  ; $f71e: 85 02
	dex                      ; $f720: ca
	bne $f710                ; $f721: d0 ed
loc_f723:
	ldy #$24                 ; $f723: a0 24
	lda #$ff                 ; $f725: a9 ff
	sta $1c                  ; $f727: 85 1c
	lda #$00                 ; $f729: a9 00
	sta $02                  ; $f72b: 85 02
	tya                      ; $f72d: 98
	and $03                  ; $f72e: 25 03
	adc $97                  ; $f730: 65 97
	and $0f                  ; $f732: 25 0f
	sta $08                  ; $f734: 85 08
	lda #$00                 ; $f736: a9 00
	sta $0d                  ; $f738: 85 0d
	lda $fd4d,y              ; $f73a: b9 4d fd
	sta $0e                  ; $f73d: 85 0e
	lda $fd72,y              ; $f73f: b9 72 fd
	sta $0f                  ; $f742: 85 0f
	lda $fd97,y              ; $f744: b9 97 fd
	sta $0d                  ; $f747: 85 0d
	lda $fdbc,y              ; $f749: b9 bc fd
	sta $0e                  ; $f74c: 85 0e
	lda #$00                 ; $f74e: a9 00
	sta $0f                  ; $f750: 85 0f
	sta $02                  ; $f752: 85 02
	dey                      ; $f754: 88
	bne $f72d                ; $f755: d0 d6
loc_f757:
	lda #$00                 ; $f757: a9 00
	sta $0e                  ; $f759: 85 0e
	lda #$00                 ; $f75b: a9 00
	ldy #$5c                 ; $f75d: a0 5c
	sta $06                  ; $f75f: 85 06
	sta $1b                  ; $f761: 85 1b
	sta $1c                  ; $f763: 85 1c
	sty $08                  ; $f765: 84 08
	sta $02                  ; $f767: 85 02
	dey                      ; $f769: 88
	bne $f75f                ; $f76a: d0 f3
loc_f76c:
	ldy #$0c                 ; $f76c: a0 0c
	sta $02                  ; $f76e: 85 02
	lda #$00                 ; $f770: a9 00
	sta $09                  ; $f772: 85 09
	dey                      ; $f774: 88
	bne $f76e                ; $f775: d0 f7
loc_f777:
	jsr $fc0c                ; $f777: 20 0c fc
loc_f77a:
	rts                      ; $f77a: 60

; === Code Block $f77b-$f7e6 ===
.org $f77b

loc_f77b:
	lda #$00                 ; $f77b: a9 00
	sta $0a                  ; $f77d: 85 0a
	inc $85                  ; $f77f: e6 85
	lda $85                  ; $f781: a5 85
	cmp #$05                 ; $f783: c9 05
	bne $f78d                ; $f785: d0 06
loc_f787:
	inc $84                  ; $f787: e6 84
	lda #$00                 ; $f789: a9 00
	sta $85                  ; $f78b: 85 85
loc_f78d:
	lda #$00                 ; $f78d: a9 00
	sta $06                  ; $f78f: 85 06
	sta $07                  ; $f791: 85 07
	ldx #$6f                 ; $f793: a2 6f
	ldy #$00                 ; $f795: a0 00
	sta $02                  ; $f797: 85 02
	dex                      ; $f799: ca
	bne $f797                ; $f79a: d0 fb
loc_f79c:
	ldy #$14                 ; $f79c: a0 14
	lda #$00                 ; $f79e: a9 00
	sta $02                  ; $f7a0: 85 02
	tya                      ; $f7a2: 98
	sbc #$0a                 ; $f7a3: e9 0a
	and #$0f                 ; $f7a5: 29 0f
	sta $08                  ; $f7a7: 85 08
	lda #$00                 ; $f7a9: a9 00
	sta $0d                  ; $f7ab: 85 0d
	lda $fdff,y              ; $f7ad: b9 ff fd
	sta $0e                  ; $f7b0: 85 0e
	lda $fe13,y              ; $f7b2: b9 13 fe
	sta $0f                  ; $f7b5: 85 0f
	lda $fe27,y              ; $f7b7: b9 27 fe
	sta $0d                  ; $f7ba: 85 0d
	lda $fe3b,y              ; $f7bc: b9 3b fe
	sta $0e                  ; $f7bf: 85 0e
	lda #$00                 ; $f7c1: a9 00
	sta $0f                  ; $f7c3: 85 0f
	sta $02                  ; $f7c5: 85 02
	dey                      ; $f7c7: 88
	bne $f7a2                ; $f7c8: d0 d8
loc_f7ca:
	lda #$00                 ; $f7ca: a9 00
	ldy #$67                 ; $f7cc: a0 67
	sta $06                  ; $f7ce: 85 06
	sta $1b                  ; $f7d0: 85 1b
	sta $1c                  ; $f7d2: 85 1c
	sta $0d                  ; $f7d4: 85 0d
	sta $0e                  ; $f7d6: 85 0e
	sta $0f                  ; $f7d8: 85 0f
	sta $08                  ; $f7da: 85 08
	sta $02                  ; $f7dc: 85 02
	dey                      ; $f7de: 88
	bne $f7ce                ; $f7df: d0 ed
loc_f7e1:
	ldy #$0c                 ; $f7e1: a0 0c
	jsr $fc0c                ; $f7e3: 20 0c fc
loc_f7e6:
	rts                      ; $f7e6: 60

; === Code Block $f7e7-$f805 ===
.org $f7e7

loc_f7e7:
	lda $85                  ; $f7e7: a5 85
	cmp #$00                 ; $f7e9: c9 00
	bne $f826                ; $f7eb: d0 39
loc_f7ed:
	lda #$71                 ; $f7ed: a9 71
	sta $0a                  ; $f7ef: 85 0a
	lda #$00                 ; $f7f1: a9 00
	sta $08                  ; $f7f3: 85 08
	sta $10                  ; $f7f5: 85 10
	lda #$07                 ; $f7f7: a9 07
	sta $04                  ; $f7f9: 85 04
	sta $05                  ; $f7fb: 85 05
	lda $80                  ; $f7fd: a5 80
	cmp #$06                 ; $f7ff: c9 06
	bne $f808                ; $f801: d0 05
loc_f803:
	lda #$34                 ; $f803: a9 34
	jmp $f80a                ; $f805: 4c 0a f8

; === Code Block $f808-$f826 ===
.org $f808

loc_f808:
	lda #$58                 ; $f808: a9 58
loc_f80a:
	ldx #$00                 ; $f80a: a2 00
	jsr $fc40                ; $f80c: 20 40 fc
loc_f80f:
	sta $20                  ; $f80f: 85 20
	lda #$2e                 ; $f811: a9 2e
	ldx #$01                 ; $f813: a2 01
	jsr $fc40                ; $f815: 20 40 fc
loc_f818:
	sty $21,x                ; $f818: 94 21
	sta $02                  ; $f81a: 85 02
	lda #$01                 ; $f81c: a9 01
	sta $85                  ; $f81e: 85 85
	lda #$01                 ; $f820: a9 01
	sta $07                  ; $f822: 85 07
	sta $06                  ; $f824: 85 06

; === Code Block $f826-$f844 ===
.org $f826

loc_f826:
	ldx #$ea                 ; $f826: a2 ea
	lda $80                  ; $f828: a5 80
	cmp #$08                 ; $f82a: c9 08
	beq $f830                ; $f82c: f0 02
loc_f82e:
	ldx #$75                 ; $f82e: a2 75
loc_f830:
	lda #$00                 ; $f830: a9 00
	sta $08                  ; $f832: 85 08
	lda #$0f                 ; $f834: a9 0f
	sbc $97                  ; $f836: e5 97
	and #$0d                 ; $f838: 29 0d
	adc #$01                 ; $f83a: 69 01
	sta $09                  ; $f83c: 85 09
	lda $80                  ; $f83e: a5 80
	cmp #$08                 ; $f840: c9 08
	beq $f847                ; $f842: f0 03
loc_f844:
	jmp $f877                ; $f844: 4c 77 f8

; === Code Block $f847-$f874 ===
.org $f847

loc_f847:
	tya                      ; $f847: 98
	adc $f1f2,y              ; $f848: 79 f2 f1
	adc $f1f2,x              ; $f84b: 7d f2 f1
	adc $81                  ; $f84e: 65 81
	asl                      ; $f850: 0a
	clc                      ; $f851: 18
	tay                      ; $f852: a8
	lda #$00                 ; $f853: a9 00
	cpy #$6e                 ; $f855: c0 6e
	bcs $f85c                ; $f857: b0 03
loc_f859:
	lda $fc78,y              ; $f859: b9 78 fc
loc_f85c:
	sta $0f                  ; $f85c: 85 0f
	sta $0e                  ; $f85e: 85 0e
	sta $0d                  ; $f860: 85 0d
	txa                      ; $f862: 8a
	lda $f1f2,x              ; $f863: bd f2 f1
	clc                      ; $f866: 18
	asl                      ; $f867: 0a
	adc $81                  ; $f868: 65 81
	tay                      ; $f86a: a8
	sta $02                  ; $f86b: 85 02
	lda #$0c                 ; $f86d: a9 0c
	sta $08                  ; $f86f: 85 08
	dex                      ; $f871: ca
	bne $f847                ; $f872: d0 d3
loc_f874:
	jmp $f8c2                ; $f874: 4c c2 f8

; === Code Block $f877-$f8c9 ===
.org $f877

loc_f877:
	lda $80                  ; $f877: a5 80
	cmp #$06                 ; $f879: c9 06
	beq $f881                ; $f87b: f0 04
loc_f87d:
	lda #$c0                 ; $f87d: a9 c0
	sta $0f                  ; $f87f: 85 0f
loc_f881:
	lda #$08                 ; $f881: a9 08
	sta $0c                  ; $f883: 85 0c
	lda #$02                 ; $f885: a9 02
	txa                      ; $f887: 8a
	lda $f1f2,x              ; $f888: bd f2 f1
	sbc $81                  ; $f88b: e5 81
	tay                      ; $f88d: a8
	tya                      ; $f88e: 98
	sbc $f1f2,y              ; $f88f: f9 f2 f1
	adc $81                  ; $f892: 65 81
	asl                      ; $f894: 0a
	rol                      ; $f895: 2a
	tay                      ; $f896: a8
	lda #$00                 ; $f897: a9 00
	cpy #$6e                 ; $f899: c0 6e
	bcs $f8a0                ; $f89b: b0 03
loc_f89d:
	lda $fc78,y              ; $f89d: b9 78 fc
loc_f8a0:
	sta $1b                  ; $f8a0: 85 1b
	txa                      ; $f8a2: 8a
	lda $f1f2,x              ; $f8a3: bd f2 f1
	adc $81                  ; $f8a6: 65 81
	tay                      ; $f8a8: a8
	tya                      ; $f8a9: 98
	sbc $f1f2,y              ; $f8aa: f9 f2 f1
	sbc $81                  ; $f8ad: e5 81
	asl                      ; $f8af: 0a
	rol                      ; $f8b0: 2a
	tay                      ; $f8b1: a8
	lda #$00                 ; $f8b2: a9 00
	cpy #$6e                 ; $f8b4: c0 6e
	bcs $f8bb                ; $f8b6: b0 03
loc_f8b8:
	lda $fc78,y              ; $f8b8: b9 78 fc
loc_f8bb:
	sta $1c                  ; $f8bb: 85 1c
	dex                      ; $f8bd: ca
	sta $02                  ; $f8be: 85 02
	bne $f877                ; $f8c0: d0 b5
loc_f8c2:
	sta $02                  ; $f8c2: 85 02
	lda #$00                 ; $f8c4: a9 00
	jsr $fc0c                ; $f8c6: 20 0c fc
loc_f8c9:
	rts                      ; $f8c9: 60

; === Code Block $f8ca-$f920 ===
.org $f8ca

loc_f8ca:
	lda #$00                 ; $f8ca: a9 00
	sta $1f                  ; $f8cc: 85 1f
	sta $1d                  ; $f8ce: 85 1d
	sta $1e                  ; $f8d0: 85 1e
	sta $10                  ; $f8d2: 85 10
	sta $11                  ; $f8d4: 85 11
	lda #$01                 ; $f8d6: a9 01
	sta $0a                  ; $f8d8: 85 0a
	lda #$04                 ; $f8da: a9 04
	sta $07                  ; $f8dc: 85 07
	sta $08                  ; $f8de: 85 08
	lda #$02                 ; $f8e0: a9 02
	sta $06                  ; $f8e2: 85 06
	lda #$3f                 ; $f8e4: a9 3f
	sta $04                  ; $f8e6: 85 04
	sta $05                  ; $f8e8: 85 05
	lda #$00                 ; $f8ea: a9 00
	ldx #$00                 ; $f8ec: a2 00
	jsr $fc40                ; $f8ee: 20 40 fc
loc_f8f1:
	sty $20,x                ; $f8f1: 94 20
	lda #$5b                 ; $f8f3: a9 5b
	ldx #$01                 ; $f8f5: a2 01
	jsr $fc40                ; $f8f7: 20 40 fc
loc_f8fa:
	sty $21,x                ; $f8fa: 94 21
	sta $02                  ; $f8fc: 85 02
	lda #$00                 ; $f8fe: a9 00
	ldx #$1d                 ; $f900: a2 1d
	ldy #$00                 ; $f902: a0 00
	dex                      ; $f904: ca
	sta $02                  ; $f905: 85 02
	bne $f904                ; $f907: d0 fb
loc_f909:
	ldy #$ab                 ; $f909: a0 ab
	lda #$00                 ; $f90b: a9 00
	cpy #$ab                 ; $f90d: c0 ab
	bcs $f913                ; $f90f: b0 02
loc_f911:
	lda #$ff                 ; $f911: a9 ff
loc_f913:
	sta $1b                  ; $f913: 85 1b
	sta $1c                  ; $f915: 85 1c
	lda $80                  ; $f917: a5 80
	cmp #$04                 ; $f919: c9 04
	beq $f923                ; $f91b: f0 06
loc_f91d:
	tya                      ; $f91d: 98
	sbc $81                  ; $f91e: e5 81
	jmp $f926                ; $f920: 4c 26 f9

; === Code Block $f923-$f941 ===
.org $f923

loc_f923:
	tya                      ; $f923: 98
	adc $81                  ; $f924: 65 81
loc_f926:
	and $20                  ; $f926: 25 20
	sta $20                  ; $f928: 85 20
	ldx #$01                 ; $f92a: a2 01
	sta $21                  ; $f92c: 85 21
	rol                      ; $f92e: 2a
	ora $92                  ; $f92f: 05 92
	lsr                      ; $f931: 4a
	sbc #$0b                 ; $f932: e9 0b
	sta $09                  ; $f934: 85 09
	sta $02                  ; $f936: 85 02
	sta $2a                  ; $f938: 85 2a
	dey                      ; $f93a: 88
	bne $f90b                ; $f93b: d0 ce
loc_f93d:
	lda #$00                 ; $f93d: a9 00
	ldy #$08                 ; $f93f: a0 08
	jmp $f944                ; $f941: 4c 44 f9

; === Code Block $f944-$f95f ===
.org $f944

loc_f944:
	sta $06                  ; $f944: 85 06
	sta $07                  ; $f946: 85 07
	sta $1b                  ; $f948: 85 1b
	sta $1c                  ; $f94a: 85 1c
	sta $09                  ; $f94c: 85 09
	sta $08                  ; $f94e: 85 08
	dey                      ; $f950: 88
	sta $02                  ; $f951: 85 02
	bne $f944                ; $f953: d0 ef
loc_f955:
	ldy #$16                 ; $f955: a0 16
	sta $02                  ; $f957: 85 02
	dey                      ; $f959: 88
	bne $f957                ; $f95a: d0 fb
loc_f95c:
	jsr $fc0c                ; $f95c: 20 0c fc
loc_f95f:
	rts                      ; $f95f: 60

; === Code Block $f9a5-$f9c2 ===
.org $f9a5

loc_f9a5:
	lda #$00                 ; $f9a5: a9 00
	sta $08                  ; $f9a7: 85 08
	sta $02                  ; $f9a9: 85 02
	ldx #$00                 ; $f9ab: a2 00
	lsr $a7                  ; $f9ad: 46 a7
	bcc $f9b3                ; $f9af: 90 02
loc_f9b1:
	ldx #$05                 ; $f9b1: a2 05
loc_f9b3:
	ldy $a7                  ; $f9b3: a4 a7
	sty $1c                  ; $f9b5: 84 1c
	sty $1b                  ; $f9b7: 84 1b
	stx $19                  ; $f9b9: 86 19
	stx $1a                  ; $f9bb: 86 1a
	dec $a8                  ; $f9bd: c6 a8
	beq $f9c3                ; $f9bf: f0 02
loc_f9c1:
	clc                      ; $f9c1: 18
	rts                      ; $f9c2: 60

; === Code Block $f9c3-$f9ce ===
.org $f9c3

loc_f9c3:
	ldx $aa                  ; $f9c3: a6 aa
	inx                      ; $f9c5: e8
	cpx $ab                  ; $f9c6: e4 ab
	bne $f9ef                ; $f9c8: d0 25
loc_f9ca:
	dec $a6                  ; $f9ca: c6 a6
	bne $f9e5                ; $f9cc: d0 17

; === Code Block $f9ce-$f9fd ===
.org $f9ce

loc_f9ce:
	ldy $a9                  ; $f9ce: a4 a9
	lda $fafa,y              ; $f9d0: b9 fa fa
	beq $f9d7                ; $f9d3: f0 02
loc_f9d5:
	inc $a9                  ; $f9d5: e6 a9
loc_f9d7:
	pha                      ; $f9d7: 48
	and #$0f                 ; $f9d8: 29 0f
	sta $a5                  ; $f9da: 85 a5
	pla                      ; $f9dc: 68
	and #$f0                 ; $f9dd: 29 f0
	lsr                      ; $f9df: 4a
	lsr                      ; $f9e0: 4a
	sta $a6                  ; $f9e1: 85 a6
	pha                      ; $f9e3: 48
	pla                      ; $f9e4: 68
	ldy $a5                  ; $f9e5: a4 a5
	ldx $fbb9,y              ; $f9e7: be b9 fb
	lda $fbba,y              ; $f9ea: b9 ba fb
	sta $ab                  ; $f9ed: 85 ab
	stx $aa                  ; $f9ef: 86 aa
	lda $fb78,x              ; $f9f1: bd 78 fb
	sta $a7                  ; $f9f4: 85 a7
	lda #$08                 ; $f9f6: a9 08
	sta $a8                  ; $f9f8: 85 a8
	pha                      ; $f9fa: 48
	pla                      ; $f9fb: 68
	sec                      ; $f9fc: 38
	rts                      ; $f9fd: 60

; === Code Block $fa0b-$fa3b ===
.org $fa0b

loc_fa0b:
	sta $02                  ; $fa0b: 85 02
	ldx #$08                 ; $fa0d: a2 08
	ldy $fa07,x              ; $fa0f: bc 07 fa
	sty $15,x                ; $fa12: 94 15
	ldy $f9fe,x              ; $fa14: bc fe f9
	sty $04,x                ; $fa17: 94 04
	dex                      ; $fa19: ca
	bpl $fa0f                ; $fa1a: 10 f3
loc_fa1c:
	nop                      ; $fa1c: ea
	nop                      ; $fa1d: ea
	ldx $00                  ; $fa1e: a6 00
	lda #$41                 ; $fa20: a9 41
	jsr $fc40                ; $fa22: 20 40 fc
loc_fa25:
	ldx $01                  ; $fa25: a6 01
	lda #$55                 ; $fa27: a9 55
	jsr $fc40                ; $fa29: 20 40 fc
loc_fa2c:
	ldx $af                  ; $fa2c: a6 af
	inc $af                  ; $fa2e: e6 af
	lda $f960,x              ; $fa30: bd 60 f9
	bne $fa3c                ; $fa33: d0 07
loc_fa35:
	inc $80                  ; $fa35: e6 80
	lda #$01                 ; $fa37: a9 01
	sta $ce                  ; $fa39: 85 ce
	rts                      ; $fa3b: 60

; === Code Block $fa3c-$fa8a ===
.org $fa3c

loc_fa3c:
	sta $a9                  ; $fa3c: 85 a9
	sta $ad                  ; $fa3e: 85 ad
	jsr $f9ce                ; $fa40: 20 ce f9
loc_fa43:
	ldx #$00                 ; $fa43: a2 00
	stx $00                  ; $fa45: 86 00
	lda #$1e                 ; $fa47: a9 1e
	jsr $fa79                ; $fa49: 20 79 fa
loc_fa4c:
	lda #$04                 ; $fa4c: a9 04
	sta $ae                  ; $fa4e: 85 ae
	jsr $fa8b                ; $fa50: 20 8b fa
loc_fa53:
	dec $ae                  ; $fa53: c6 ae
	bpl $fa50                ; $fa55: 10 f9
loc_fa57:
	sta $02                  ; $fa57: 85 02
	lda #$1b                 ; $fa59: a9 1b
	jsr $fa79                ; $fa5b: 20 79 fa
loc_fa5e:
	sta $02                  ; $fa5e: 85 02
	sta $02                  ; $fa60: 85 02
	jsr $f9a5                ; $fa62: 20 a5 f9
loc_fa65:
	bcs $fa69                ; $fa65: b0 02
loc_fa67:
	sta $02                  ; $fa67: 85 02
loc_fa69:
	ldx #$02                 ; $fa69: a2 02
	stx $00                  ; $fa6b: 86 00
	jsr $fab7                ; $fa6d: 20 b7 fa
loc_fa70:
	ldx $a9                  ; $fa70: a6 a9
	lda $fafa,x              ; $fa72: bd fa fa
	bne $fa43                ; $fa75: d0 cc
loc_fa77:
	beq $fa2c                ; $fa77: f0 b3
loc_fa79:
	sta $ae                  ; $fa79: 85 ae
	jsr $f9a5                ; $fa7b: 20 a5 f9
loc_fa7e:
	bcs $fa82                ; $fa7e: b0 02
loc_fa80:
	sta $02                  ; $fa80: 85 02
loc_fa82:
	sta $02                  ; $fa82: 85 02
	sta $02                  ; $fa84: 85 02
	dec $ae                  ; $fa86: c6 ae
	bne $fa7b                ; $fa88: d0 f1
loc_fa8a:
	rts                      ; $fa8a: 60

; === Code Block $fa8b-$fab6 ===
.org $fa8b

loc_fa8b:
	jsr $f9a5                ; $fa8b: 20 a5 f9
loc_fa8e:
	bcs $fa93                ; $fa8e: b0 03
loc_fa90:
	jsr $fa96                ; $fa90: 20 96 fa
loc_fa93:
	jsr $fa96                ; $fa93: 20 96 fa
loc_fa96:
	sta $02                  ; $fa96: 85 02
	lda #$0f                 ; $fa98: a9 0f
	sta $08                  ; $fa9a: 85 08
	ldx $ae                  ; $fa9c: a6 ae
	lda $b0,x                ; $fa9e: b5 b0
	sta $0d                  ; $faa0: 85 0d
	lda $b5,x                ; $faa2: b5 b5
	sta $0e                  ; $faa4: 85 0e
	lda $ba,x                ; $faa6: b5 ba
	sta $0f                  ; $faa8: 85 0f
	lda $bf,x                ; $faaa: b5 bf
	sta $0d                  ; $faac: 85 0d
	lda $c4,x                ; $faae: b5 c4
	sta $0e                  ; $fab0: 85 0e
	lda $c9,x                ; $fab2: b5 c9
	sta $0f                  ; $fab4: 85 0f
	rts                      ; $fab6: 60

; === Code Block $fab7-$faed ===
.org $fab7

loc_fab7:
	ldy $ac                  ; $fab7: a4 ac
	lda $fbc9,y              ; $fab9: b9 c9 fb
	ldx #$04                 ; $fabc: a2 04
	lsr                      ; $fabe: 4a
	pha                      ; $fabf: 48
	ror $c9,x                ; $fac0: 76 c9
	rol $c4,x                ; $fac2: 36 c4
	lda $bf,x                ; $fac4: b5 bf
	ror                      ; $fac6: 6a
	sta $bf,x                ; $fac7: 95 bf
	lsr                      ; $fac9: 4a
	lsr                      ; $faca: 4a
	lsr                      ; $facb: 4a
	lsr                      ; $facc: 4a
	ror $ba,x                ; $facd: 76 ba
	rol $b5,x                ; $facf: 36 b5
	ror $b0,x                ; $fad1: 76 b0
	pla                      ; $fad3: 68
	dex                      ; $fad4: ca
	bpl $fabe                ; $fad5: 10 e7
loc_fad7:
	lda $fbc9,y              ; $fad7: b9 c9 fb
	bpl $faf0                ; $fada: 10 14
loc_fadc:
	asl                      ; $fadc: 0a
	beq $fae3                ; $fadd: f0 04
loc_fadf:
	ldy #$2b                 ; $fadf: a0 2b
	bne $faf7                ; $fae1: d0 14
loc_fae3:
	ldx $ad                  ; $fae3: a6 ad
	ldy $faf9,x              ; $fae5: bc f9 fa
	beq $faf9                ; $fae8: f0 0f
loc_faea:
	dex                      ; $faea: ca
	stx $ad                  ; $faeb: 86 ad
	jmp $faf7                ; $faed: 4c f7 fa

; === Code Block $faf0-$faf9 ===
.org $faf0

loc_faf0:
	ldy $ac                  ; $faf0: a4 ac
	bpl $faf6                ; $faf2: 10 02
loc_faf4:
	ldy #$ff                 ; $faf4: a0 ff
loc_faf6:
	iny                      ; $faf6: c8
loc_faf7:
	sty $ac                  ; $faf7: 84 ac
loc_faf9:
	rts                      ; $faf9: 60

; === Code Block $fbfa-$fc0b ===
.org $fbfa

loc_fbfa:
	lda #$02                 ; $fbfa: a9 02
	sta $00                  ; $fbfc: 85 00
	sta $02                  ; $fbfe: 85 02
	sta $02                  ; $fc00: 85 02
	lda #$18                 ; $fc02: a9 18
	sta $0296                ; $fc04: 8d 96 02
	sta $02                  ; $fc07: 85 02
	sta $00                  ; $fc09: 85 00
	rts                      ; $fc0b: 60

; === Code Block $fc0c-$fc15 ===
.org $fc0c

loc_fc0c:
	txa                      ; $fc0c: 8a
	sbc $97                  ; $fc0d: e5 97
	sta $17                  ; $fc0f: 85 17
	lda $97                  ; $fc11: a5 97
	sta $16                  ; $fc13: 85 16
	rts                      ; $fc15: 60

; === Code Block $fc16-$fc3f ===
.org $fc16

loc_fc16:
	lda #$00                 ; $fc16: a9 00
	sta $0d                  ; $fc18: 85 0d
	sta $0e                  ; $fc1a: 85 0e
	sta $0f                  ; $fc1c: 85 0f
	sta $09                  ; $fc1e: 85 09
	sta $1d                  ; $fc20: 85 1d
	sta $1e                  ; $fc22: 85 1e
	sta $1f                  ; $fc24: 85 1f
	lda #$00                 ; $fc26: a9 00
	sta $19                  ; $fc28: 85 19
	sta $1a                  ; $fc2a: 85 1a
	lda $80                  ; $fc2c: a5 80
	cmp #$11                 ; $fc2e: c9 11
	bcs $fc3f                ; $fc30: b0 0d
loc_fc32:
	lda $99                  ; $fc32: a5 99
	cmp #$17                 ; $fc34: c9 17
	bne $fc3c                ; $fc36: d0 04
loc_fc38:
	lda #$07                 ; $fc38: a9 07
	sta $99                  ; $fc3a: 85 99
loc_fc3c:
	jsr $f1bb                ; $fc3c: 20 bb f1
loc_fc3f:
	rts                      ; $fc3f: 60

; === Code Block $fc40-$fc53 ===
.org $fc40

loc_fc40:
	sta $02                  ; $fc40: 85 02
	sec                      ; $fc42: 38
	sbc #$0f                 ; $fc43: e9 0f
	bcs $fc43                ; $fc45: b0 fc
loc_fc47:
	tay                      ; $fc47: a8
	lda $ef0f,y              ; $fc48: b9 0f ef
	sta $20,x                ; $fc4b: 95 20
	sta $10,x                ; $fc4d: 95 10
	sta $02                  ; $fc4f: 85 02
	sta $2a                  ; $fc51: 85 2a
	rts                      ; $fc53: 60

; === Vectors ===
.org $fffc
	.word reset
	.word reset
