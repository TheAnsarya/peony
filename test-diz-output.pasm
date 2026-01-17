; 🌺 Peony Disassembly
; ROM: Dragon Warrior (U) (PRG1) [!].nes
; Platform: NES
; Size: 81936 bytes
; Mapper: MMC1
; Labels: 2407

; === Block $8000-$8003 (Code) ===
Bank0_Funca9:
	jsr loc_c5e0             ; $8000: 20 e0 c5
loc_8003:
	jmp loc_c009             ; $8003: 4c 09 c0

; === Block $8020-$8024 (Code) ===
loc_8020:
	sec                      ; $8020: 38
	rol                      ; $8021: 2a
	asl $08                  ; $8022: 06 08

; === Block $8024-$8031 (Code) ===
loc_8024:
	dey                      ; $8024: 88
	bne loc_8020             ; $8025: d0 f9
loc_8027:
	and ($0a),y              ; $8027: 31 0a
	ora $08                  ; $8029: 05 08
	sta ($0a),y              ; $802b: 91 0a
	sta $08                  ; $802d: 85 08
	pla                      ; $802f: 68
	tay                      ; $8030: a8
	rts                      ; $8031: 60

; === Block $8028-$8061 (Code) ===
loc_8028:
	*nop $5f,x               ; $8028: f4 5f
	ora ($0a),y              ; $802a: 11 0a
	ora $5f11,x              ; $802c: 1d 11 5f
	jsr $1418                ; $802f: 20 18 14
loc_8032:
	asl $5f17                ; $8032: 0e 17 5f
	asl $4719,x              ; $8035: 1e 19 47
	*nop $fd60,x             ; $8038: fc 60 fd
	*rla $11,x               ; $803b: 37 11
	clc                      ; $803d: 18
	asl $0a5f,x              ; $803e: 1e 5f 0a
	*slo $5f1d,y             ; $8041: 1b 1d 5f
loc_8044:
	ora $0a0e                ; $8044: 0d 0e 0a
loc_8047:
	ora $fc47                ; $8047: 0d 47 fc
	bvc loc_8083             ; $804a: 50 37
loc_804c:
	ora ($18),y              ; $804c: 11 18
	asl $0a5f,x              ; $804e: 1e 5f 0a
	*slo $5f1d,y             ; $8051: 1b 1d 5f
	*nop $1b1d,x             ; $8054: 1c 1d 1b
	clc                      ; $8057: 18
loc_8058:
	*slo $10,x               ; $8058: 17 10
	*sre $170e,x             ; $805a: 5f 0e 17
	clc                      ; $805d: 18
	asl $1110,x              ; $805e: 1e 10 11
	jmp $3afd                ; $8061: 4c fd 3a PPUSCROLL

; === Block $8028-$8029 (Code) ===
loc_8028:
	asl                      ; $8028: 0a

; === Block $8042-$8046 (Code) ===
	bne loc_8047             ; $8042: d0 03
loc_8044:
	lda #$00                 ; $8044: a9 00
	rts                      ; $8046: 60

; === Block $8045-$8045 (Code) ===
	brk                      ; $8045: 00

; === Block $8047-$8049 (Code) ===
loc_8047:
	lda #$01                 ; $8047: a9 01
	rts                      ; $8049: 60

; === Block $8048-$806f (Code) ===
	ora ($60,x)              ; $8048: 01 60
	sta $26                  ; $804a: 85 26
	tya                      ; $804c: 98
	pha                      ; $804d: 48
	txa                      ; $804e: 8a
	pha                      ; $804f: 48
	ldx $26                  ; $8050: a6 26
	lda $602f                ; $8052: ad 2f 60
	jsr loc_c032             ; $8055: 20 32 c0
loc_8058:
	sta $25                  ; $8058: 85 25
	lsr                      ; $805a: 4a
	ror                      ; $805b: 6a
	ror                      ; $805c: 6a
	ror                      ; $805d: 6a
	sta $24                  ; $805e: 85 24
	lda $52,x                ; $8060: b5 52
	and #$9f                 ; $8062: 29 9f
	ora $24                  ; $8064: 05 24
	sta $52,x                ; $8066: 95 52
	lda #$00                 ; $8068: a9 00
	sta $27                  ; $806a: 85 27
	ldy $0200                ; $806c: ac 00 02

; === Block $804b-$804d (Code) ===
	rol $98                  ; $804b: 26 98

; === Block $8053-$805b (Code) ===
	*rla $2060               ; $8053: 2f 60 20 PPUCTRL
	*jam                     ; $8056: 32
	cpy #$85                 ; $8057: c0 85
	and $4a                  ; $8059: 25 4a

; === Block $8069-$8069 (Code) ===
loc_8069:
	brk                      ; $8069: 00

; === Block $806d-$806d (Code) ===
	brk                      ; $806d: 00

; === Block $807a-$807c (Code) ===
	bpl loc_8024             ; $807a: 10 a8
loc_807c:
	jmp loc_c09c             ; $807c: 4c 9c c0

; === Block $807e-$8088 (Code) ===
	cpy #$c9                 ; $807e: c0 c9
	ora ($d0,x)              ; $8080: 01 d0
	php                      ; $8082: 08
	txa                      ; $8083: 8a
	clc                      ; $8084: 18
	adc #$10                 ; $8085: 69 10
	tax                      ; $8087: aa
	jmp loc_c09c             ; $8088: 4c 9c c0

; === Block $808c-$8093 (Code) ===
	*jam                     ; $808c: 02
	bne loc_8097             ; $808d: d0 08
loc_808f:
	tya                      ; $808f: 98
	clc                      ; $8090: 18
	adc #$10                 ; $8091: 69 10

; === Block $8097-$8098 (Code) ===
loc_8097:
	txa                      ; $8097: 8a

; === Block $8098-$80c3 (Code) ===
	sec                      ; $8098: 38
	sbc #$10                 ; $8099: e9 10
	tax                      ; $809b: aa
	stx $22                  ; $809c: 86 22
	sty $23                  ; $809e: 84 23
	ldy #$10                 ; $80a0: a0 10
	lda $0200,y              ; $80a2: b9 00 02
	cmp $23                  ; $80a5: c5 23
	bne loc_80b0             ; $80a7: d0 07
loc_80a9:
	lda $0203,y              ; $80a9: b9 03 02
	cmp $22                  ; $80ac: c5 22
	beq loc_80c6             ; $80ae: f0 16
loc_80b0:
	tya                      ; $80b0: 98
	clc                      ; $80b1: 18
	adc #$10                 ; $80b2: 69 10
	tay                      ; $80b4: a8
	bne $80a2                ; $80b5: d0 eb
loc_80b7:
	ldx $22                  ; $80b7: a6 22
	ldy $23                  ; $80b9: a4 23
	lda $27                  ; $80bb: a5 27
	bne loc_80ef             ; $80bd: d0 30
loc_80bf:
	lda #$01                 ; $80bf: a9 01
	sta $27                  ; $80c1: 85 27
	jmp loc_c072             ; $80c3: 4c 72 c0

; === Block $809d-$809e (Code) ===
	*jam                     ; $809d: 22

; === Block $80aa-$80ac (Code) ===
	*slo ($02,x)             ; $80aa: 03 02

; === Block $80b3-$80b5 (Code) ===
	bpl $805d                ; $80b3: 10 a8

; === Block $80b6-$80b9 (Code) ===
	*sbc #$a6                ; $80b6: eb a6
	*jam                     ; $80b8: 22

; === Block $80bc-$80f3 (Code) ===
	*rla $d0                 ; $80bc: 27 d0
	bmi loc_8069             ; $80be: 30 a9
loc_80c0:
	ora ($85,x)              ; $80c0: 01 85
	*rla $4c                 ; $80c2: 27 4c
	*jam                     ; $80c4: 72
	cpy #$84                 ; $80c5: c0 84
	plp                      ; $80c7: 28
	lda #$04                 ; $80c8: a9 04
	sta $27                  ; $80ca: 85 27
	ldx $26                  ; $80cc: a6 26
	jsr loc_c0f4             ; $80ce: 20 f4 c0
loc_80d1:
	tay                      ; $80d1: a8
	lda $25                  ; $80d2: a5 25
	jsr loc_b6c2             ; $80d4: 20 c2 b6
loc_80d7:
	ldx $28                  ; $80d7: a6 28
	lda ($22),y              ; $80d9: b1 22
	sta $0201,x              ; $80db: 9d 01 02
	iny                      ; $80de: c8
	lda ($22),y              ; $80df: b1 22
	dey                      ; $80e1: 88
	sta $0202,x              ; $80e2: 9d 02 02
	inx                      ; $80e5: e8
	inx                      ; $80e6: e8
	inx                      ; $80e7: e8
	inx                      ; $80e8: e8
	iny                      ; $80e9: c8
	iny                      ; $80ea: c8
	dec $27                  ; $80eb: c6 27
	bne $80d9                ; $80ed: d0 ea
loc_80ef:
	pla                      ; $80ef: 68
	tax                      ; $80f0: aa
	pla                      ; $80f1: 68
	tay                      ; $80f2: a8
	rts                      ; $80f3: 60

; === Block $80c6-$80c8 (Code) ===
loc_80c6:
	sty $28                  ; $80c6: 84 28

; === Block $80cf-$80d1 (Code) ===
	*nop $c0,x               ; $80cf: f4 c0

; === Block $80d6-$80d9 (Code) ===
	ldx $a6,y                ; $80d6: b6 a6
	plp                      ; $80d8: 28

; === Block $80dc-$80de (Code) ===
	ora ($02,x)              ; $80dc: 01 02

; === Block $80e4-$80e5 (Code) ===
	*jam                     ; $80e4: 02

; === Block $80ec-$80ef (Code) ===
	*rla $d0                 ; $80ec: 27 d0
	nop                      ; $80ee: ea

; === Block $80f6-$813b (Code) ===
	and #$e0                 ; $80f6: 29 e0
	lsr                      ; $80f8: 4a
	sta $24                  ; $80f9: 85 24
	cmp #$60                 ; $80fb: c9 60
	bne loc_8117             ; $80fd: d0 18
loc_80ff:
	lda $45                  ; $80ff: a5 45
	cmp #$04                 ; $8101: c9 04
	bne loc_810b             ; $8103: d0 06
loc_8105:
	lda $e4                  ; $8105: a5 e4
	and #$04                 ; $8107: 29 04
	bne loc_8111             ; $8109: d0 06
loc_810b:
	lda $45                  ; $810b: a5 45
	cmp #$05                 ; $810d: c9 05
	bne loc_814b             ; $810f: d0 3a
loc_8111:
	lda #$d0                 ; $8111: a9 d0
	sta $24                  ; $8113: 85 24
	bne loc_814b             ; $8115: d0 34
loc_8117:
	lda $24                  ; $8117: a5 24
	cmp #$50                 ; $8119: c9 50
	bne loc_8147             ; $811b: d0 2a
loc_811d:
	lda $45                  ; $811d: a5 45
	cmp #$04                 ; $811f: c9 04
	bne loc_813b             ; $8121: d0 18
loc_8123:
	lda $e4                  ; $8123: a5 e4
	and #$04                 ; $8125: 29 04
	beq loc_813b             ; $8127: f0 12
loc_8129:
	lda #$f0                 ; $8129: a9 f0
	sta $24                  ; $812b: 85 24
	lda $c7                  ; $812d: a5 c7
	cmp #$ff                 ; $812f: c9 ff
	bne loc_8153             ; $8131: d0 20
loc_8133:
	lda $24                  ; $8133: a5 24
	ora #$08                 ; $8135: 09 08
	sta $24                  ; $8137: 85 24
	bne loc_8153             ; $8139: d0 18

; === Block $80fc-$80fc (Code) ===
	rts                      ; $80fc: 60

; === Block $8100-$810c (Code) ===
	eor $c9                  ; $8100: 45 c9
	*nop $d0                 ; $8102: 04 d0
	asl $a5                  ; $8104: 06 a5
	cpx $29                  ; $8106: e4 29
	*nop $d0                 ; $8108: 04 d0
	asl $a5                  ; $810a: 06 a5

; === Block $811a-$811d (Code) ===
	bvc $80ec                ; $811a: 50 d0
loc_811c:
	rol                      ; $811c: 2a

; === Block $811e-$8123 (Code) ===
	eor $c9                  ; $811e: 45 c9
	*nop $d0                 ; $8120: 04 d0
	clc                      ; $8122: 18

; === Block $8124-$8129 (Code) ===
	cpx $29                  ; $8124: e4 29
	*nop $f0                 ; $8126: 04 f0
	*jam                     ; $8128: 12

; === Block $812a-$8133 (Code) ===
	beq $80b1                ; $812a: f0 85
loc_812c:
	bit $a5                  ; $812c: 24 a5
	*dcp $c9                 ; $812e: c7 c9
	*isc $20d0,x             ; $8130: ff d0 20 PPUCTRL

; === Block $8132-$8135 (Code) ===
	jsr $24a5                ; $8132: 20 a5 24 PPUSCROLL

; === Block $8138-$813a (Code) ===
	bit $d0                  ; $8138: 24 d0

; === Block $813d-$8155 (Code) ===
	cmp #$06                 ; $813d: c9 06
	bne loc_814b             ; $813f: d0 0a
loc_8141:
	lda #$e0                 ; $8141: a9 e0
	sta $24                  ; $8143: 85 24
	bne loc_814b             ; $8145: d0 04
loc_8147:
	cmp #$70                 ; $8147: c9 70
	beq $812d                ; $8149: f0 e2
loc_814b:
	lda $50                  ; $814b: a5 50
	and #$08                 ; $814d: 29 08
	ora $24                  ; $814f: 05 24
	sta $24                  ; $8151: 85 24
loc_8153:
	lda $24                  ; $8153: a5 24
	rts                      ; $8155: 60

; === Block $8140-$8141 (Code) ===
	asl                      ; $8140: 0a

; === Block $8156-$816f (Code) ===
	ldx #$3a                 ; $8156: a2 3a
	lda #$1e                 ; $8158: a9 1e
	sta $c7                  ; $815a: 85 c7
	lda $ba                  ; $815c: a5 ba
	sec                      ; $815e: 38
	sbc $f35b,x              ; $815f: fd 5b f3
	lda $bb                  ; $8162: a5 bb
	sbc $f35c,x              ; $8164: fd 5c f3
	bcs loc_816f             ; $8167: b0 06
loc_8169:
	dec $c7                  ; $8169: c6 c7
	dex                      ; $816b: ca
	dex                      ; $816c: ca
	bne $815c                ; $816d: d0 ed
loc_816f:
	rts                      ; $816f: 60

; === Block $8171-$8175 (Code) ===
	bit $20                  ; $8171: 24 20
	*nop $ff,x               ; $8173: 74 ff

; === Block $8172-$8175 (Code) ===
loc_8172:
	jsr loc_ff74             ; $8172: 20 74 ff

; === Block $8174-$8179 (Code) ===
	*isc $24c6,x             ; $8174: ff c6 24 PPUADDR
	bpl loc_8172             ; $8177: 10 f9
loc_8179:
	rts                      ; $8179: 60

; === Block $8178-$81b5 (Code) ===
	sbc $4860,y              ; $8178: f9 60 48
	txa                      ; $817b: 8a
	pha                      ; $817c: 48
	tya                      ; $817d: 98
	pha                      ; $817e: 48
	jsr loc_ff74             ; $817f: 20 74 ff
loc_8182:
	lda #$08                 ; $8182: a9 08
	sta $2000                ; $8184: 8d 00 20 PPUCTRL
	lda #$5f                 ; $8187: a9 5f
	sta $08                  ; $8189: 85 08
	lda $2002                ; $818b: ad 02 20 PPUSTATUS
	lda #$20                 ; $818e: a9 20
	sta $2006                ; $8190: 8d 06 20 PPUADDR
	lda #$00                 ; $8193: a9 00
	sta $2006                ; $8195: 8d 06 20 PPUADDR
	jsr loc_c1b9             ; $8198: 20 b9 c1
loc_819b:
	lda $2002                ; $819b: ad 02 20 PPUSTATUS
	lda #$24                 ; $819e: a9 24
	sta $2006                ; $81a0: 8d 06 20 PPUADDR
	lda #$00                 ; $81a3: a9 00
	sta $2006                ; $81a5: 8d 06 20 PPUADDR
	jsr loc_c1b9             ; $81a8: 20 b9 c1
loc_81ab:
	lda #$88                 ; $81ab: a9 88
	sta $2000                ; $81ad: 8d 00 20 PPUCTRL
	jsr loc_ff74             ; $81b0: 20 74 ff
loc_81b3:
	pla                      ; $81b3: 68
	tay                      ; $81b4: a8

; === Block $817a-$817b (Code) ===
	pha                      ; $817a: 48

; === Block $8183-$8184 (Code) ===
	php                      ; $8183: 08

; === Block $8196-$8198 (Code) ===
	asl $20                  ; $8196: 06 20

; === Block $8199-$81a0 (Code) ===
	lda $adc1,y              ; $8199: b9 c1 ad
	*jam                     ; $819c: 02
	jsr $24a9                ; $819d: 20 a9 24 PPUMASK

; === Block $819f-$81a3 (Code) ===
	bit $8d                  ; $819f: 24 8d
	asl $20                  ; $81a1: 06 20

; === Block $81a2-$81a5 (Code) ===
	jsr $00a9                ; $81a2: 20 a9 00

; === Block $81a7-$81ad (Code) ===
	jsr loc_b920             ; $81a7: 20 20 b9
loc_81aa:
	cmp ($a9,x)              ; $81aa: c1 a9
	dey                      ; $81ac: 88

; === Block $81bd-$81bf (Code) ===
loc_81bd:
	ldy #$20                 ; $81bd: a0 20

; === Block $81bf-$81c2 (Code) ===
loc_81bf:
	sta $2007                ; $81bf: 8d 07 20 PPUDATA

; === Block $81c0-$81c8 (Code) ===
	*slo $20                 ; $81c0: 07 20
	dey                      ; $81c2: 88
	bne loc_81bf             ; $81c3: d0 fa
loc_81c5:
	dex                      ; $81c5: ca
	bne loc_81bd             ; $81c6: d0 f5
loc_81c8:
	rts                      ; $81c8: 60

; === Block $81c4-$81c5 (Code) ===
	*nop                     ; $81c4: fa

; === Block $8220-$8243 (Code) ===
loc_8220:
	sta $0c                  ; $8220: 85 0c
	lda $3f                  ; $8222: a5 3f
	sta $0d                  ; $8224: 85 0d
	jsr loc_c632             ; $8226: 20 32 c6
loc_8229:
	lda $3d                  ; $8229: a5 3d
	beq loc_8238             ; $822b: f0 0b
loc_822d:
	lda $40                  ; $822d: a5 40
	sta $0c                  ; $822f: 85 0c
	lda $41                  ; $8231: a5 41
	sta $0d                  ; $8233: 85 0d
	jsr loc_c63d             ; $8235: 20 3d c6
loc_8238:
	lda $3c                  ; $8238: a5 3c
	clc                      ; $823a: 18
	adc #$10                 ; $823b: 69 10
	sta $3c                  ; $823d: 85 3c
	cmp #$50                 ; $823f: c9 50
	bne loc_8216             ; $8241: d0 d3
loc_8243:
	rts                      ; $8243: 60

; === Block $8246-$8256 (Code) ===
	asl                      ; $8246: 0a
	clc                      ; $8247: 18
	adc $0f                  ; $8248: 65 0f
	and #$3f                 ; $824a: 29 3f
	pha                      ; $824c: 48
	lda $4b                  ; $824d: a5 4b
	asl                      ; $824f: 0a
	clc                      ; $8250: 18
	adc $10                  ; $8251: 65 10
	clc                      ; $8253: 18
	adc #$1e                 ; $8254: 69 1e

; === Block $8249-$824c (Code) ===
	*slo $3f29               ; $8249: 0f 29 3f PPUMASK

; === Block $824e-$8250 (Code) ===
	*alr #$0a                ; $824e: 4b 0a

; === Block $8259-$8269 (Code) ===
	asl $3e85,x              ; $8259: 1e 85 3e PPUSCROLL
	jsr loc_c1f0             ; $825c: 20 f0 c1
loc_825f:
	lda $40                  ; $825f: a5 40
	sta $3e                  ; $8261: 85 3e
	pla                      ; $8263: 68
	sta $3c                  ; $8264: 85 3c
	jsr loc_c270             ; $8266: 20 70 c2
loc_8269:
	rts                      ; $8269: 60

; === Block $825d-$825f (Code) ===
	beq loc_8220             ; $825d: f0 c1

; === Block $8304-$8325 (Code) ===
	lda $e5                  ; $8304: a5 e5
	asl                      ; $8306: 0a
	asl                      ; $8307: 0a
	asl                      ; $8308: 0a
	sta $3c                  ; $8309: 85 3c
	asl                      ; $830b: 0a
	adc $3c                  ; $830c: 65 3c
	adc #$03                 ; $830e: 69 03
	tax                      ; $8310: aa
	lda #$01                 ; $8311: a9 01
	sta $02                  ; $8313: 85 02
	lda $02                  ; $8315: a5 02
	jsr loc_ff74             ; $8317: 20 74 ff
loc_831a:
	bne $8315                ; $831a: d0 f9
loc_831c:
	jsr loc_c608             ; $831c: 20 08 c6
loc_831f:
	lda $47                  ; $831f: a5 47
	and #$08                 ; $8321: 29 08
	bne $8311                ; $8323: d0 ec

; === Block $830f-$8311 (Code) ===
	*slo ($aa,x)             ; $830f: 03 aa

; === Block $8314-$8315 (Code) ===
	*jam                     ; $8314: 02

; === Block $8316-$8317 (Code) ===
	*jam                     ; $8316: 02

; === Block $8318-$831a (Code) ===
	*nop $ff,x               ; $8318: 74 ff

; === Block $831b-$8323 (Code) ===
	sbc $0820,y              ; $831b: f9 20 08
	dec $a5                  ; $831e: c6 a5
	*sre $29                 ; $8320: 47 29
	php                      ; $8322: 08

; === Block $831d-$831e (Code) ===
	php                      ; $831d: 08

; === Block $833a-$833a (Code) ===
	brk                      ; $833a: 00

; === Block $833c-$833d (Code) ===
	txs                      ; $833c: 9a
	jmp loc_c474             ; $833d: 4c 74 c4

; === Block $833f-$834c (Code) ===
	cpy $c9                  ; $833f: c4 c9
	*nop                     ; $8341: fa
	bne loc_834d             ; $8342: d0 09
loc_8344:
	lda $9b                  ; $8344: a5 9b
	sta $99                  ; $8346: 85 99
	lda $9c                  ; $8348: a5 9c
	sta $9a                  ; $834a: 85 9a
	rts                      ; $834c: 60

; === Block $8347-$834a (Code) ===
	sta $9ca5,y              ; $8347: 99 a5 9c

; === Block $834d-$834f (Code) ===
loc_834d:
	cmp #$f0                 ; $834d: c9 f0

; === Block $834f-$8359 (Code) ===
	bne loc_8382             ; $834f: d0 31
loc_8351:
	iny                      ; $8351: c8
	lda ($99),y              ; $8352: b1 99
	sta $3e                  ; $8354: 85 3e
	iny                      ; $8356: c8
	lda ($99),y              ; $8357: b1 99

; === Block $8355-$8358 (Code) ===
	rol $b1c8,x              ; $8355: 3e c8 b1

; === Block $8360-$836a (Code) ===
	and $3eb1,x              ; $8360: 3d b1 3e PPUMASK
	sta $3c                  ; $8363: 85 3c
	pla                      ; $8365: 68
	tay                      ; $8366: a8
	jsr loc_c6c9             ; $8367: 20 c9 c6

; === Block $8368-$836a (Code) ===
	cmp #$c6                 ; $8368: c9 c6

; === Block $836b-$8371 (Code) ===
	sta $6918,y              ; $836b: 99 18 69
	*jam                     ; $836e: 02
	sta $9b                  ; $836f: 85 9b

; === Block $836d-$836f (Code) ===
	adc #$02                 ; $836d: 69 02

; === Block $8384-$8389 (Code) ===
	bne loc_83be             ; $8384: d0 38
loc_8386:
	jsr loc_c38c             ; $8386: 20 8c c3
loc_8389:
	jmp loc_c474             ; $8389: 4c 74 c4

; === Block $838b-$8396 (Code) ===
	cpy $c8                  ; $838b: c4 c8
	lda ($99),y              ; $838d: b1 99
	sta $3e                  ; $838f: 85 3e
	iny                      ; $8391: c8
	lda ($99),y              ; $8392: b1 99
	sta $3f                  ; $8394: 85 3f

; === Block $838e-$8391 (Code) ===
	sta $3e85,y              ; $838e: 99 85 3e PPUSCROLL

; === Block $8390-$8396 (Code) ===
	rol $b1c8,x              ; $8390: 3e c8 b1
	sta $3f85,y              ; $8393: 99 85 3f PPUSCROLL

; === Block $8395-$83bd (Code) ===
	*rla $4898,x             ; $8395: 3f 98 48
	ldy #$00                 ; $8398: a0 00
	lda ($3e),y              ; $839a: b1 3e
	sta $3c                  ; $839c: 85 3c
	iny                      ; $839e: c8
	lda ($3e),y              ; $839f: b1 3e
	sta $3d                  ; $83a1: 85 3d
	pla                      ; $83a3: 68
	tay                      ; $83a4: a8
	jsr loc_c6c9             ; $83a5: 20 c9 c6
loc_83a8:
	lda $99                  ; $83a8: a5 99
	clc                      ; $83aa: 18
	adc #$02                 ; $83ab: 69 02
	sta $9b                  ; $83ad: 85 9b
	lda $9a                  ; $83af: a5 9a
	adc #$00                 ; $83b1: 69 00
	sta $9c                  ; $83b3: 85 9c
	lda #$00                 ; $83b5: a9 00
	sta $9a                  ; $83b7: 85 9a
	lda #$af                 ; $83b9: a9 af
	sta $99                  ; $83bb: 85 99
	rts                      ; $83bd: 60

; === Block $839b-$839e (Code) ===
	rol $3c85,x              ; $839b: 3e 85 3c PPUSCROLL

; === Block $839d-$83a3 (Code) ===
	*nop $b1c8,x             ; $839d: 3c c8 b1
	rol $3d85,x              ; $83a0: 3e 85 3d PPUSCROLL

; === Block $83a2-$83a5 (Code) ===
	and $a868,x              ; $83a2: 3d 68 a8

; === Block $83a7-$83ad (Code) ===
	dec $a5                  ; $83a7: c6 a5
	sta $6918,y              ; $83a9: 99 18 69
	*jam                     ; $83ac: 02

; === Block $83b4-$83b7 (Code) ===
	*shy $00a9,x             ; $83b4: 9c a9 00

; === Block $83ba-$83bd (Code) ===
	*lax $9985               ; $83ba: af 85 99

; === Block $83bc-$83c6 (Code) ===
	sta $c960,y              ; $83bc: 99 60 c9
	*isc ($d0),y             ; $83bf: f3 d0
	*slo ($20),y             ; $83c1: 13 20
	sty $a0c3                ; $83c3: 8c c3 a0
	brk                      ; $83c6: 00

; === Block $83be-$83c2 (Code) ===
loc_83be:
	cmp #$f3                 ; $83be: c9 f3
	bne loc_83d5             ; $83c0: d0 13

; === Block $83c2-$83c9 (Code) ===
	jsr loc_c38c             ; $83c2: 20 8c c3
loc_83c5:
	ldy #$00                 ; $83c5: a0 00
	lda ($99),y              ; $83c7: b1 99

; === Block $83c4-$83c6 (Code) ===
	*dcp ($a0,x)             ; $83c4: c3 a0

; === Block $83c8-$83cf (Code) ===
	sta $5fc9,y              ; $83c8: 99 c9 5f
	bne loc_83d2             ; $83cb: d0 05
loc_83cd:
	inc $99                  ; $83cd: e6 99
	jmp loc_c3c7             ; $83cf: 4c c7 c3

; === Block $83ca-$83cd (Code) ===
	*sre $05d0,x             ; $83ca: 5f d0 05

; === Block $83d0-$83d2 (Code) ===
	*dcp $c3                 ; $83d0: c7 c3
loc_83d2:
	jmp loc_c474             ; $83d2: 4c 74 c4

; === Block $83d5-$83d9 (Code) ===
loc_83d5:
	cmp #$f2                 ; $83d5: c9 f2
	bne loc_83ec             ; $83d7: d0 13

; === Block $83f5-$8407 (Code) ===
	bvc loc_83e8             ; $83f5: 50 f1
loc_83f7:
	sta $3c                  ; $83f7: 85 3c
	lda $f151                ; $83f9: ad 51 f1
	sta $3d                  ; $83fc: 85 3d
	ldy #$00                 ; $83fe: a0 00
	lda ($3c),y              ; $8400: b1 3c
	cmp #$fa                 ; $8402: c9 fa
	beq loc_840a             ; $8404: f0 04
loc_8406:
	iny                      ; $8406: c8
	jmp loc_c400             ; $8407: 4c 00 c4

; === Block $83fa-$83fc (Code) ===
	eor ($f1),y              ; $83fa: 51 f1

; === Block $8401-$8404 (Code) ===
	*nop $fac9,x             ; $8401: 3c c9 fa

; === Block $8403-$8404 (Code) ===
	*nop                     ; $8403: fa

; === Block $8408-$8408 (Code) ===
	brk                      ; $8408: 00

; === Block $840a-$840b (Code) ===
loc_840a:
	dex                      ; $840a: ca

; === Block $840b-$840f (Code) ===
	beq loc_841a             ; $840b: f0 0d
loc_840d:
	tya                      ; $840d: 98
	sec                      ; $840e: 38

; === Block $8415-$8417 (Code) ===
	inc $3d                  ; $8415: e6 3d
	jmp loc_c3fe             ; $8417: 4c fe c3

; === Block $8419-$842a (Code) ===
	*dcp ($a5,x)             ; $8419: c3 a5
	sta $9b85,y              ; $841b: 99 85 9b
	lda $9a                  ; $841e: a5 9a
	sta $9c                  ; $8420: 85 9c
	lda $3c                  ; $8422: a5 3c
	sta $99                  ; $8424: 85 99
	lda $3d                  ; $8426: a5 3d
	sta $9a                  ; $8428: 85 9a
	jmp loc_c474             ; $842a: 4c 74 c4

; === Block $841a-$841e (Code) ===
loc_841a:
	lda $99                  ; $841a: a5 99
	sta $9b                  ; $841c: 85 9b

; === Block $8423-$8426 (Code) ===
	*nop $9985,x             ; $8423: 3c 85 99

; === Block $8427-$842a (Code) ===
loc_8427:
	and $9a85,x              ; $8427: 3d 85 9a

; === Block $842b-$8431 (Code) ===
	*nop $c4,x               ; $842b: 74 c4
	cmp #$57                 ; $842d: c9 57
	beq loc_8434             ; $842f: f0 03
loc_8431:
	jmp loc_c474             ; $8431: 4c 74 c4

; === Block $8434-$8436 (Code) ===
loc_8434:
	lda $d4                  ; $8434: a5 d4

; === Block $8446-$845b (Code) ===
	and #$03                 ; $8446: 29 03
	beq loc_844e             ; $8448: f0 04
loc_844a:
	lda #$5f                 ; $844a: a9 5f
	bne loc_8456             ; $844c: d0 08
loc_844e:
	lda $4f                  ; $844e: a5 4f
	and #$10                 ; $8450: 29 10
	bne loc_844a             ; $8452: d0 f6
loc_8454:
	lda #$57                 ; $8454: a9 57
loc_8456:
	sta $08                  ; $8456: 85 08
	jsr loc_ff74             ; $8458: 20 74 ff

; === Block $8449-$844e (Code) ===
	*nop $a9                 ; $8449: 04 a9
	*sre $08d0,x             ; $844b: 5f d0 08

; === Block $844d-$844e (Code) ===
	php                      ; $844d: 08

; === Block $844f-$8452 (Code) ===
	*sre $1029               ; $844f: 4f 29 10

; === Block $8451-$8458 (Code) ===
	bpl $8423                ; $8451: 10 d0
loc_8453:
	inc $a9,x                ; $8453: f6 a9
	*sre $85,x               ; $8455: 57 85
	php                      ; $8457: 08

; === Block $8459-$845b (Code) ===
	*nop $ff,x               ; $8459: 74 ff

; === Block $845c-$8461 (Code) ===
	sbc $c4,x                ; $845c: f5 c4
	jsr loc_c690             ; $845e: 20 90 c6

; === Block $845f-$8461 (Code) ===
	bcc loc_8427             ; $845f: 90 c6

; === Block $848f-$84a2 (Code) ===
	sta $f8c9,y              ; $848f: 99 c9 f8
	beq loc_849e             ; $8492: f0 0a
loc_8494:
	cmp #$f9                 ; $8494: c9 f9
	bne loc_84ea             ; $8496: d0 52
loc_8498:
	lda #$52                 ; $8498: a9 52
	sta $08                  ; $849a: 85 08
	bne loc_84a2             ; $849c: d0 04
loc_849e:
	lda #$51                 ; $849e: a9 51
	sta $08                  ; $84a0: 85 08

; === Block $8491-$8492 (Code) ===
	sed                      ; $8491: f8

; === Block $8499-$849a (Code) ===
	*jam                     ; $8499: 52

; === Block $849b-$849c (Code) ===
	php                      ; $849b: 08

; === Block $84b1-$84c3 (Code) ===
	dec $43                  ; $84b1: c6 43
	lda $09                  ; $84b3: a5 09
	beq loc_84c1             ; $84b5: f0 0a
loc_84b7:
	cmp #$01                 ; $84b7: c9 01
	beq loc_84c1             ; $84b9: f0 06
loc_84bb:
	lda $08                  ; $84bb: a5 08
	ldy #$00                 ; $84bd: a0 00
	sta ($42),y              ; $84bf: 91 42
loc_84c1:
	lda $42                  ; $84c1: a5 42

; === Block $84b8-$84bd (Code) ===
	ora ($f0,x)              ; $84b8: 01 f0
	asl $a5                  ; $84ba: 06 a5
	php                      ; $84bc: 08

; === Block $84c0-$84c1 (Code) ===
	*jam                     ; $84c0: 42

; === Block $84c2-$84c3 (Code) ===
	*jam                     ; $84c2: 42

; === Block $84e7-$84e7 (Code) ===
	brk                      ; $84e7: 00

; === Block $84e9-$84ec (Code) ===
	tya                      ; $84e9: 98
loc_84ea:
	inc $42                  ; $84ea: e6 42

; === Block $8510-$8528 (Code) ===
	lda #$00                 ; $8510: a9 00
	sta $08                  ; $8512: 85 08
	jsr loc_c006             ; $8514: 20 06 c0
loc_8517:
	jsr loc_c273             ; $8517: 20 73 c2
loc_851a:
	pla                      ; $851a: 68
	sta $08                  ; $851b: 85 08
	lda $97                  ; $851d: a5 97
	sta $3c                  ; $851f: 85 3c
	lda $98                  ; $8521: a5 98
	sta $3e                  ; $8523: 85 3e
	jsr loc_c5aa             ; $8525: 20 aa c5
loc_8528:
	rts                      ; $8528: 60

; === Block $8513-$8514 (Code) ===
	php                      ; $8513: 08

; === Block $8515-$8517 (Code) ===
	asl $c0                  ; $8515: 06 c0

; === Block $8518-$851a (Code) ===
	*rra ($c2),y             ; $8518: 73 c2

; === Block $851c-$851d (Code) ===
	php                      ; $851c: 08

; === Block $851e-$8523 (Code) ===
	*sax $85,y               ; $851e: 97 85
	*nop $98a5,x             ; $8520: 3c a5 98

; === Block $8526-$8542 (Code) ===
	tax                      ; $8526: aa
	cmp $60                  ; $8527: c5 60
	lda #$30                 ; $8529: a9 30
	sta $3c                  ; $852b: 85 3c
	ldx #$04                 ; $852d: a2 04
	jsr loc_ff74             ; $852f: 20 74 ff
loc_8532:
	dex                      ; $8532: ca
	bne $852f                ; $8533: d0 fa
loc_8535:
	lda $3e                  ; $8535: a5 3e
	sta $0c                  ; $8537: 85 0c
	lda $3f                  ; $8539: a5 3f
	sta $0d                  ; $853b: 85 0d
	jsr loc_c632             ; $853d: 20 32 c6
loc_8540:
	lda $3d                  ; $8540: a5 3d

; === Block $852e-$8532 (Code) ===
	*nop $20                 ; $852e: 04 20
	*nop $ff,x               ; $8530: 74 ff

; === Block $8531-$8535 (Code) ===
	*isc loc_d0ca,x          ; $8531: ff ca d0
	*nop                     ; $8534: fa

; === Block $8536-$8539 (Code) ===
	rol $0c85,x              ; $8536: 3e 85 0c

; === Block $8538-$853b (Code) ===
	*nop $3fa5               ; $8538: 0c a5 3f PPUSCROLL

; === Block $853e-$8544 (Code) ===
	*jam                     ; $853e: 32
	dec $a5                  ; $853f: c6 a5
	and $0bf0,x              ; $8541: 3d f0 0b

; === Block $854d-$855a (Code) ===
	and $a5c6,x              ; $854d: 3d c6 a5
	*nop $e938,x             ; $8550: 3c 38 e9
	bpl loc_84da             ; $8553: 10 85
loc_8555:
	*nop $f0c9,x             ; $8555: 3c c9 f0
	bne $852d                ; $8558: d0 d3
loc_855a:
	rts                      ; $855a: 60

; === Block $854f-$8558 (Code) ===
	lda $3c                  ; $854f: a5 3c
	sec                      ; $8551: 38
	sbc #$10                 ; $8552: e9 10
	sta $3c                  ; $8554: 85 3c
	cmp #$f0                 ; $8556: c9 f0

; === Block $855b-$8586 (Code) ===
	lda $95                  ; $855b: a5 95
	sta $3d                  ; $855d: 85 3d
	lda $94                  ; $855f: a5 94
	sta $3c                  ; $8561: 85 3c
	asl $94                  ; $8563: 06 94
	rol $95                  ; $8565: 26 95
	clc                      ; $8567: 18
	adc $94                  ; $8568: 65 94
	sta $94                  ; $856a: 85 94
	lda $95                  ; $856c: a5 95
	adc $3d                  ; $856e: 65 3d
	sta $95                  ; $8570: 85 95
	lda $94                  ; $8572: a5 94
	clc                      ; $8574: 18
	adc $95                  ; $8575: 65 95
	sta $95                  ; $8577: 85 95
	lda $94                  ; $8579: a5 94
	clc                      ; $857b: 18
	adc #$81                 ; $857c: 69 81
	sta $94                  ; $857e: 85 94
	lda $95                  ; $8580: a5 95
	adc #$00                 ; $8582: 69 00
	sta $95                  ; $8584: 85 95
	rts                      ; $8586: 60

; === Block $8562-$8565 (Code) ===
	*nop $9406,x             ; $8562: 3c 06 94

; === Block $856b-$8572 (Code) ===
	sty $a5,x                ; $856b: 94 a5
	sta $65,x                ; $856d: 95 65
	and $9585,x              ; $856f: 3d 85 95

; === Block $8571-$8575 (Code) ===
	sta $a5,x                ; $8571: 95 a5
	sty $18,x                ; $8573: 94 18

; === Block $8576-$857c (Code) ===
	sta $85,x                ; $8576: 95 85
	sta $a5,x                ; $8578: 95 a5
	sty $18,x                ; $857a: 94 18

; === Block $857d-$8583 (Code) ===
	sta ($85,x)              ; $857d: 81 85
	sty $a5,x                ; $857f: 94 a5
	sta $69,x                ; $8581: 95 69
	brk                      ; $8583: 00

; === Block $8589-$8592 (Code) ===
	lda $04                  ; $8589: a5 04
	cmp $24                  ; $858b: c5 24
	bcc loc_8595             ; $858d: 90 06
loc_858f:
	jsr loc_ff74             ; $858f: 20 74 ff
loc_8592:
	jmp loc_c589             ; $8592: 4c 89 c5

; === Block $858e-$8592 (Code) ===
	asl $20                  ; $858e: 06 20
	*nop $ff,x               ; $8590: 74 ff

; === Block $8593-$8595 (Code) ===
	*nop #$c5                ; $8593: 89 c5
loc_8595:
	rts                      ; $8595: 60

; === Block $8596-$85b2 (Code) ===
	lda #$40                 ; $8596: a9 40
	ora $3c                  ; $8598: 05 3c
	sta $3c                  ; $859a: 85 3c
	bne loc_85a6             ; $859c: d0 08
loc_859e:
	lda #$80                 ; $859e: a9 80
	ora $3c                  ; $85a0: 05 3c
	sta $3c                  ; $85a2: 85 3c
	bne loc_85aa             ; $85a4: d0 04
loc_85a6:
	asl $3c                  ; $85a6: 06 3c
	asl $3e                  ; $85a8: 06 3e
loc_85aa:
	lda $3e                  ; $85aa: a5 3e
	sta $0b                  ; $85ac: 85 0b
	lda #$00                 ; $85ae: a9 00
	sta $0a                  ; $85b0: 85 0a

; === Block $8599-$859c (Code) ===
	*nop $3c85,x             ; $8599: 3c 85 3c PPUSCROLL

; === Block $859b-$859e (Code) ===
	*nop $08d0,x             ; $859b: 3c d0 08

; === Block $85a7-$85aa (Code) ===
	*nop $3e06,x             ; $85a7: 3c 06 3e PPUADDR

; === Block $85a9-$85ac (Code) ===
	rol $3ea5,x              ; $85a9: 3e a5 3e PPUSCROLL

; === Block $85ab-$85ae (Code) ===
	rol $0b85,x              ; $85ab: 3e 85 0b

; === Block $85bc-$85c3 (Code) ===
	ror $0a                  ; $85bc: 66 0a
	lda $3c                  ; $85be: a5 3c
	and #$1f                 ; $85c0: 29 1f
	clc                      ; $85c2: 18

; === Block $85bf-$85c2 (Code) ===
	*nop $1f29,x             ; $85bf: 3c 29 1f

; === Block $85c1-$85c4 (Code) ===
	*slo $6518,x             ; $85c1: 1f 18 65

; === Block $85de-$85fd (Code) ===
	*anc #$60                ; $85de: 0b 60
	asl $3c                  ; $85e0: 06 3c
	asl $3e                  ; $85e2: 06 3e
	lda $3e                  ; $85e4: a5 3e
	and #$fc                 ; $85e6: 29 fc
	asl                      ; $85e8: 0a
	sta $0a                  ; $85e9: 85 0a
	lda $3c                  ; $85eb: a5 3c
	and #$1f                 ; $85ed: 29 1f
	lsr                      ; $85ef: 4a
	lsr                      ; $85f0: 4a
	clc                      ; $85f1: 18
	adc $0a                  ; $85f2: 65 0a
	clc                      ; $85f4: 18
	adc #$c0                 ; $85f5: 69 c0
	sta $0a                  ; $85f7: 85 0a
	lda $3c                  ; $85f9: a5 3c
	and #$20                 ; $85fb: 29 20

; === Block $85e1-$85e4 (Code) ===
	*nop $3e06,x             ; $85e1: 3c 06 3e PPUADDR

; === Block $85f6-$85f9 (Code) ===
	cpy #$85                 ; $85f6: c0 85
	asl                      ; $85f8: 0a

; === Block $85fa-$85fd (Code) ===
	*nop $2029,x             ; $85fa: 3c 29 20 PPUMASK

; === Block $8655-$8660 (Code) ===
	dec $20                  ; $8655: c6 20
	adc ($c6,x)              ; $8657: 61 c6
	jsr loc_c661             ; $8659: 20 61 c6
loc_865c:
	cpy #$0c                 ; $865c: c0 0c
	bne loc_864c             ; $865e: d0 ec
loc_8660:
	rts                      ; $8660: 60

; === Block $8658-$865c (Code) ===
	dec $20                  ; $8658: c6 20
	adc ($c6,x)              ; $865a: 61 c6

; === Block $865b-$8660 (Code) ===
	dec $c0                  ; $865b: c6 c0
	*nop $ecd0               ; $865d: 0c d0 ec

; === Block $8663-$8667 (Code) ===
	cmp #$01                 ; $8663: c9 01
	beq loc_8671             ; $8665: f0 0a

; === Block $8796-$8798 (Code) ===
	*nop $a9                 ; $8796: 04 a9
	brk                      ; $8798: 00

; === Block $879b-$87a7 (Code) ===
	cmp #$ca                 ; $879b: c9 ca
	bcs loc_87a3             ; $879d: b0 04
loc_879f:
	lda #$01                 ; $879f: a9 01
	bne loc_87ad             ; $87a1: d0 0a
loc_87a3:
	cmp #$de                 ; $87a3: c9 de
	bcs loc_87ab             ; $87a5: b0 04

; === Block $87a0-$87a3 (Code) ===
	ora ($d0,x)              ; $87a0: 01 d0
	asl                      ; $87a2: 0a

; === Block $87a4-$87a7 (Code) ===
	dec $04b0,x              ; $87a4: de b0 04

; === Block $8835-$883c (Code) ===
	cmp $a5                  ; $8835: c5 a5
	asl                      ; $8837: 0a
	sta $42                  ; $8838: 85 42
	lda $0b                  ; $883a: a5 0b

; === Block $883f-$8846 (Code) ===
	lda #$5f                 ; $883f: a9 5f
	sta $08                  ; $8841: 85 08
	jsr loc_c4f5             ; $8843: 20 f5 c4
loc_8846:
	jmp loc_c690             ; $8846: 4c 90 c6

; === Block $8848-$884c (Code) ===
	dec $a9                  ; $8848: c6 a9
	lsr $85,x                ; $884a: 56 85

; === Block $885e-$886a (Code) ===
	*sre $48                 ; $885e: 47 48
	jsr loc_c608             ; $8860: 20 08 c6
loc_8863:
	pla                      ; $8863: 68
	beq loc_8871             ; $8864: f0 0b
loc_8866:
	lda $4f                  ; $8866: a5 4f
	and #$0f                 ; $8868: 29 0f

; === Block $8861-$8864 (Code) ===
	php                      ; $8861: 08
	dec $68                  ; $8862: c6 68

; === Block $8867-$886a (Code) ===
	*sre $0f29               ; $8867: 4f 29 0f

; === Block $8881-$8881 (Code) ===
	brk                      ; $8881: 00

; === Block $8883-$8892 (Code) ===
	*dcp $a5,x               ; $8883: d7 a5
	cmp $6518,y              ; $8885: d9 18 65
	*dcp $85,x               ; $8888: d7 85
	*dcp $a9,x               ; $888a: d7 a9
	sta $00                  ; $888c: 85 00
	*nop $17                 ; $888e: 04 17
	lda $d7                  ; $8890: a5 d7
	rts                      ; $8892: 60

; === Block $88d6-$88e9 (Code) ===
	lda #$00                 ; $88d6: a9 00
	sta $d9                  ; $88d8: 85 d9
	lda $9d                  ; $88da: a5 9d
	sta $97                  ; $88dc: 85 97
	lda $9e                  ; $88de: a5 9e
	sec                      ; $88e0: 38
	sbc #$02                 ; $88e1: e9 02
	cmp #$fe                 ; $88e3: c9 fe
	bne loc_88e9             ; $88e5: d0 02
loc_88e7:
	lda #$1c                 ; $88e7: a9 1c

; === Block $8aaf-$8ab6 (Code) ===
	cmp #$01                 ; $8aaf: c9 01
	beq loc_8ab9             ; $8ab1: f0 06
loc_8ab3:
	jsr loc_b6da             ; $8ab3: 20 da b6
loc_8ab6:
	jmp loc_caa8             ; $8ab6: 4c a8 ca

; === Block $8ab9-$8ad2 (Code) ===
loc_8ab9:
	jsr loc_c608             ; $8ab9: 20 08 c6
loc_8abc:
	lda $47                  ; $8abc: a5 47
	and #$08                 ; $8abe: 29 08
	bne loc_8ab9             ; $8ac0: d0 f7
loc_8ac2:
	jsr loc_c608             ; $8ac2: 20 08 c6
loc_8ac5:
	lda $47                  ; $8ac5: a5 47
	and #$08                 ; $8ac7: 29 08
	beq loc_8ac2             ; $8ac9: f0 f7
loc_8acb:
	jsr loc_c608             ; $8acb: 20 08 c6
loc_8ace:
	lda $47                  ; $8ace: a5 47
	and #$08                 ; $8ad0: 29 08

; === Block $8aca-$8ad4 (Code) ===
	*isc $20,x               ; $8aca: f7 20
	php                      ; $8acc: 08
	dec $a5                  ; $8acd: c6 a5
	*sre $29                 ; $8acf: 47 29
	php                      ; $8ad1: 08
	bne loc_8acb             ; $8ad2: d0 f7
loc_8ad4:
	jmp loc_ca9b             ; $8ad4: 4c 9b ca

; === Block $8b58-$8b68 (Code) ===
loc_8b58:
	jsr loc_c632             ; $8b58: 20 32 c6
loc_8b5b:
	lda #$30                 ; $8b5b: a9 30
	sta $3c                  ; $8b5d: 85 3c
	jsr loc_c63d             ; $8b5f: 20 3d c6
loc_8b62:
	jsr loc_ff74             ; $8b62: 20 74 ff
loc_8b65:
	jsr loc_f050             ; $8b65: 20 50 f0

; === Block $8b66-$8b75 (Code) ===
	bvc loc_8b58             ; $8b66: 50 f0
loc_8b68:
	jsr loc_fc98             ; $8b68: 20 98 fc
loc_8b6b:
	lda $cf                  ; $8b6b: a5 cf
	and #$c0                 ; $8b6d: 29 c0
	beq loc_8b78             ; $8b6f: f0 07
loc_8b71:
	lda #$01                 ; $8b71: a9 01
	sta $c5                  ; $8b73: 85 c5
	jmp loc_cb96             ; $8b75: 4c 96 cb

; === Block $8b78-$8b8e (Code) ===
loc_8b78:
	lda $603a                ; $8b78: ad 3a 60
	cmp #$78                 ; $8b7b: c9 78
	bne loc_8b96             ; $8b7d: d0 17
loc_8b7f:
	txa                      ; $8b7f: 8a
	pha                      ; $8b80: 48
	lda $ca                  ; $8b81: a5 ca
	sta $c5                  ; $8b83: 85 c5
	lda $cb                  ; $8b85: a5 cb
	sta $c6                  ; $8b87: 85 c6
	ldx $6039                ; $8b89: ae 39 60
	lda #$ab                 ; $8b8c: a9 ab

; === Block $8b96-$8b9a (Code) ===
loc_8b96:
	lda #$03                 ; $8b96: a9 03
	sta $3a                  ; $8b98: 85 3a

; === Block $8c39-$8c39 (Code) ===
	brk                      ; $8c39: 00

; === Block $8c3b-$8c5c (Code) ===
	sty $7420                ; $8c3b: 8c 20 74
	*isc $da20,x             ; $8c3e: ff 20 da
	ldx $20,y                ; $8c41: b6 20
	cmp $c7                  ; $8c43: c5 c7
	*nop $7420,x             ; $8c45: 1c 20 74
	*isc $7420,x             ; $8c48: ff 20 74
	*isc $8ca5,x             ; $8c4b: ff a5 8c
	clc                      ; $8c4e: 18
	adc #$10                 ; $8c4f: 69 10
	sta $8c                  ; $8c51: 85 8c
	bcc loc_8c57             ; $8c53: 90 02
loc_8c55:
	inc $8a                  ; $8c55: e6 8a
loc_8c57:
	jsr loc_b6da             ; $8c57: 20 da b6
loc_8c5a:
	lda $8a                  ; $8c5a: a5 8a

; === Block $8c91-$8ced (Code) ===
	*dcp $1e                 ; $8c91: c7 1e
	jsr loc_c6f0             ; $8c93: 20 f0 c6
loc_8c96:
	ora #$f0                 ; $8c96: 09 f0
	*slo $20                 ; $8c98: 07 20
	*axs #$c7                ; $8c9a: cb c7
	ldx $4c,y                ; $8c9c: b6 4c
	*sax $20cc               ; $8c9e: 8f cc 20 OAMDATA
	*axs #$c7                ; $8ca1: cb c7
	clv                      ; $8ca3: b8
	lda #$00                 ; $8ca4: a9 00
	sta $8a                  ; $8ca6: 85 8a
	sta $8b                  ; $8ca8: 85 8b
	sta $8c                  ; $8caa: 85 8c
	lda $df                  ; $8cac: a5 df
	ora #$01                 ; $8cae: 09 01
	sta $df                  ; $8cb0: 85 df
	jsr loc_ff74             ; $8cb2: 20 74 ff
loc_8cb5:
	jsr loc_b6da             ; $8cb5: 20 da b6
loc_8cb8:
	jsr loc_c7c5             ; $8cb8: 20 c5 c7
loc_8cbb:
	*jam                     ; $8cbb: 22
	ldx #$78                 ; $8cbc: a2 78
	jsr loc_ff74             ; $8cbe: 20 74 ff
loc_8cc1:
	dex                      ; $8cc1: ca
	bne $8cbe                ; $8cc2: d0 fa
loc_8cc4:
	lda #$02                 ; $8cc4: a9 02
	jsr loc_a7a2             ; $8cc6: 20 a2 a7
loc_8cc9:
	lda #$01                 ; $8cc9: a9 01
	sta $602f                ; $8ccb: 8d 2f 60
	jsr loc_b6da             ; $8cce: 20 da b6
loc_8cd1:
	lda #$1e                 ; $8cd1: a9 1e
	jsr loc_c170             ; $8cd3: 20 70 c1
loc_8cd6:
	lda #$02                 ; $8cd6: a9 02
	sta $602f                ; $8cd8: 8d 2f 60
	jsr loc_b6da             ; $8cdb: 20 da b6
loc_8cde:
	ldx #$1e                 ; $8cde: a2 1e
	jsr loc_ff74             ; $8ce0: 20 74 ff
loc_8ce3:
	dex                      ; $8ce3: ca
	bne $8ce0                ; $8ce4: d0 fa
loc_8ce6:
	lda #$ff                 ; $8ce6: a9 ff
	sta $c7                  ; $8ce8: 85 c7
	jsr loc_b6da             ; $8cea: 20 da b6

; === Block $8c94-$8c96 (Code) ===
	beq loc_8c5c             ; $8c94: f0 c6

; === Block $8c97-$8c9c (Code) ===
	beq loc_8ca0             ; $8c97: f0 07
loc_8c99:
	jsr loc_c7cb             ; $8c99: 20 cb c7

; === Block $8c9f-$8ca4 (Code) ===
	cpy loc_cb20             ; $8c9f: cc 20 cb
	*dcp $b8                 ; $8ca2: c7 b8

; === Block $8ca0-$8ca3 (Code) ===
loc_8ca0:
	jsr loc_c7cb             ; $8ca0: 20 cb c7

; === Block $8ca5-$8ca5 (Code) ===
	brk                      ; $8ca5: 00

; === Block $8ca9-$8cae (Code) ===
	*xaa #$85                ; $8ca9: 8b 85
	sty $dfa5                ; $8cab: 8c a5 df

; === Block $8cf6-$8d65 (Code) ===
	lda $be                  ; $8cf6: a5 be
	and #$1c                 ; $8cf8: 29 1c
	cmp #$1c                 ; $8cfa: c9 1c
	beq loc_8d0a             ; $8cfc: f0 0c
loc_8cfe:
	cmp #$18                 ; $8cfe: c9 18
	bne loc_8d30             ; $8d00: d0 2e
loc_8d02:
	inc $e3                  ; $8d02: e6 e3
	lda $e3                  ; $8d04: a5 e3
	and #$03                 ; $8d06: 29 03
	bne loc_8d30             ; $8d08: d0 26
loc_8d0a:
	inc $c5                  ; $8d0a: e6 c5
	lda $c5                  ; $8d0c: a5 c5
	cmp $ca                  ; $8d0e: c5 ca
	bcc loc_8d16             ; $8d10: 90 04
loc_8d12:
	lda $ca                  ; $8d12: a5 ca
	sta $c5                  ; $8d14: 85 c5
loc_8d16:
	lda $ca                  ; $8d16: a5 ca
	lsr                      ; $8d18: 4a
	lsr                      ; $8d19: 4a
	clc                      ; $8d1a: 18
	adc #$01                 ; $8d1b: 69 01
	cmp $c5                  ; $8d1d: c5 c5
	bcs loc_8d30             ; $8d1f: b0 0f
loc_8d21:
	lda #$01                 ; $8d21: a9 01
	sta $0a                  ; $8d23: 85 0a
	lda #$3f                 ; $8d25: a9 3f
	sta $0b                  ; $8d27: 85 0b
	lda #$30                 ; $8d29: a9 30
	sta $08                  ; $8d2b: 85 08
	jsr loc_c690             ; $8d2d: 20 90 c6
loc_8d30:
	lda $45                  ; $8d30: a5 45
	cmp #$04                 ; $8d32: c9 04
	bne loc_8d51             ; $8d34: d0 1b
loc_8d36:
	lda $cf                  ; $8d36: a5 cf
	and #$c0                 ; $8d38: 29 c0
	beq loc_8d51             ; $8d3a: f0 15
loc_8d3c:
	lda $3b                  ; $8d3c: a5 3b
	cmp #$1b                 ; $8d3e: c9 1b
	bne loc_8d51             ; $8d40: d0 0f
loc_8d42:
	lda #$ff                 ; $8d42: a9 ff
	sta $96                  ; $8d44: 85 96
	jsr loc_c6f0             ; $8d46: 20 f0 c6
loc_8d49:
	*jam                     ; $8d49: 02
	jsr loc_c7cb             ; $8d4a: 20 cb c7
loc_8d4d:
	*nop $4c                 ; $8d4d: 44 4c
	plp                      ; $8d4f: 28
	*jam                     ; $8d50: b2
loc_8d51:
	lda $45                  ; $8d51: a5 45
	cmp #$03                 ; $8d53: c9 03
	bne loc_8d68             ; $8d55: d0 11
loc_8d57:
	lda $3a                  ; $8d57: a5 3a
	cmp #$12                 ; $8d59: c9 12
	bne loc_8d68             ; $8d5b: d0 0b
loc_8d5d:
	lda $3b                  ; $8d5d: a5 3b
	cmp #$0c                 ; $8d5f: c9 0c
	bne loc_8d68             ; $8d61: d0 05
loc_8d63:
	lda #$21                 ; $8d63: a9 21
	jmp loc_e4df             ; $8d65: 4c df e4

; === Block $8cf7-$8cfa (Code) ===
	ldx $1c29,y              ; $8cf7: be 29 1c

; === Block $8d0f-$8d10 (Code) ===
loc_8d0f:
	dex                      ; $8d0f: ca

; === Block $8d31-$8d38 (Code) ===
	eor $c9                  ; $8d31: 45 c9
	*nop $d0                 ; $8d33: 04 d0
	*slo $cfa5,y             ; $8d35: 1b a5 cf

; === Block $8d37-$8d3a (Code) ===
	*dcp $c029               ; $8d37: cf 29 c0

; === Block $8d39-$8d40 (Code) ===
	cpy #$f0                 ; $8d39: c0 f0
	ora $a5,x                ; $8d3b: 15 a5
	*rla $1bc9,y             ; $8d3d: 3b c9 1b

; === Block $8d3f-$8d42 (Code) ===
	*slo $0fd0,y             ; $8d3f: 1b d0 0f

; === Block $8d43-$8d46 (Code) ===
	*isc $9685,x             ; $8d43: ff 85 96

; === Block $8d45-$8d49 (Code) ===
	stx $20,y                ; $8d45: 96 20
	beq loc_8d0f             ; $8d47: f0 c6

; === Block $8d48-$8d4a (Code) ===
	dec $02                  ; $8d48: c6 02

; === Block $8d4b-$8d4d (Code) ===
	*axs #$c7                ; $8d4b: cb c7

; === Block $8d4e-$8d4e (Code) ===
	jmp loc_b228             ; $8d4e: 4c 28 b2

; === Block $8d54-$8d59 (Code) ===
	*slo ($d0,x)             ; $8d54: 03 d0
	ora ($a5),y              ; $8d56: 11 a5
	*nop                     ; $8d58: 3a

; === Block $8d5a-$8d5b (Code) ===
	*jam                     ; $8d5a: 12

; === Block $8d68-$8d6a (Code) ===
loc_8d68:
	lda $45                  ; $8d68: a5 45

; === Block $8d6a-$8d82 (Code) ===
	cmp #$15                 ; $8d6a: c9 15
	bne loc_8d85             ; $8d6c: d0 17
loc_8d6e:
	lda $3a                  ; $8d6e: a5 3a
	cmp #$04                 ; $8d70: c9 04
	bne loc_8d85             ; $8d72: d0 11
loc_8d74:
	lda $3b                  ; $8d74: a5 3b
	cmp #$0e                 ; $8d76: c9 0e
	bne loc_8d85             ; $8d78: d0 0b
loc_8d7a:
	lda $e4                  ; $8d7a: a5 e4
	and #$40                 ; $8d7c: 29 40
	bne loc_8d85             ; $8d7e: d0 05
loc_8d80:
	lda #$1e                 ; $8d80: a9 1e
	jmp loc_e4df             ; $8d82: 4c df e4

; === Block $8d73-$8d78 (Code) ===
	ora ($a5),y              ; $8d73: 11 a5
	*rla $0ec9,y             ; $8d75: 3b c9 0e

; === Block $8d77-$8d7a (Code) ===
	asl $0bd0                ; $8d77: 0e d0 0b

; === Block $8d85-$8d9f (Code) ===
loc_8d85:
	lda $45                  ; $8d85: a5 45
	cmp #$01                 ; $8d87: c9 01
	bne loc_8da2             ; $8d89: d0 17
loc_8d8b:
	lda $3a                  ; $8d8b: a5 3a
	cmp #$49                 ; $8d8d: c9 49
	bne loc_8da2             ; $8d8f: d0 11
loc_8d91:
	lda $3b                  ; $8d91: a5 3b
	cmp #$64                 ; $8d93: c9 64
	bne loc_8da2             ; $8d95: d0 0b
loc_8d97:
	lda $e4                  ; $8d97: a5 e4
	and #$02                 ; $8d99: 29 02
	bne loc_8da2             ; $8d9b: d0 05
loc_8d9d:
	lda #$18                 ; $8d9d: a9 18
	jmp loc_e4df             ; $8d9f: 4c df e4

; === Block $8da2-$8da5 (Code) ===
loc_8da2:
	jsr loc_c55b             ; $8da2: 20 5b c5

; === Block $8dbd-$8dc5 (Code) ===
loc_8dbd:
	eor ($d9,x)              ; $8dbd: 41 d9
	lda $e4                  ; $8dbf: a5 e4
	and #$04                 ; $8dc1: 29 04
	beq loc_8dc6             ; $8dc3: f0 01
loc_8dc5:
	rts                      ; $8dc5: 60

; === Block $8dc6-$8dd4 (Code) ===
loc_8dc6:
	lda $e0                  ; $8dc6: a5 e0
	cmp #$06                 ; $8dc8: c9 06
	bne loc_8e02             ; $8dca: d0 36
loc_8dcc:
	lda $be                  ; $8dcc: a5 be
	and #$1c                 ; $8dce: 29 1c
	cmp #$1c                 ; $8dd0: c9 1c
	beq loc_8dfb             ; $8dd2: f0 27

; === Block $8dd4-$8dd6 (Code) ===
	lda #$84                 ; $8dd4: a9 84
	brk                      ; $8dd6: 00

; === Block $8dd8-$8df7 (Code) ===
	*slo $20,x               ; $8dd8: 17 20
	*nop $ee,x               ; $8dda: 14 ee
	jsr loc_ff74             ; $8ddc: 20 74 ff
loc_8ddf:
	lda $c5                  ; $8ddf: a5 c5
	sec                      ; $8de1: 38
	sbc #$02                 ; $8de2: e9 02
	bcs loc_8de8             ; $8de4: b0 02
loc_8de6:
	lda #$00                 ; $8de6: a9 00
loc_8de8:
	sta $c5                  ; $8de8: 85 c5
	jsr loc_ff74             ; $8dea: 20 74 ff
loc_8ded:
	jsr loc_ee28             ; $8ded: 20 28 ee
loc_8df0:
	lda $c5                  ; $8df0: a5 c5
	bne loc_8dfb             ; $8df2: d0 07
loc_8df4:
	jsr loc_c6f0             ; $8df4: 20 f0 c6
loc_8df7:
	brk                      ; $8df7: 00

; === Block $8dd9-$8ddc (Code) ===
	jsr loc_ee14             ; $8dd9: 20 14 ee

; === Block $8ddb-$8de1 (Code) ===
	inc $7420                ; $8ddb: ee 20 74
	*isc $c5a5,x             ; $8dde: ff a5 c5

; === Block $8ddd-$8ddf (Code) ===
	*nop $ff,x               ; $8ddd: 74 ff

; === Block $8de5-$8de6 (Code) ===
	*jam                     ; $8de5: 02

; === Block $8deb-$8ded (Code) ===
	*nop $ff,x               ; $8deb: 74 ff

; === Block $8dee-$8df2 (Code) ===
	plp                      ; $8dee: 28
	inc $c5a5                ; $8def: ee a5 c5

; === Block $8df1-$8df7 (Code) ===
	cmp $d0                  ; $8df1: c5 d0
	*slo $20                 ; $8df3: 07 20
	beq loc_8dbd             ; $8df5: f0 c6

; === Block $8df6-$8df8 (Code) ===
	dec $00                  ; $8df6: c6 00
	jmp loc_eda7             ; $8df8: 4c a7 ed

; === Block $8df9-$8e01 (Code) ===
	*lax $ed                 ; $8df9: a7 ed
loc_8dfb:
	lda #$0f                 ; $8dfb: a9 0f
	and $95                  ; $8dfd: 25 95
	beq loc_8e7c             ; $8dff: f0 7b
loc_8e01:
	rts                      ; $8e01: 60

; === Block $8dfc-$8dff (Code) ===
	*slo $9525               ; $8dfc: 0f 25 95

; === Block $8e02-$8e08 (Code) ===
loc_8e02:
	cmp #$01                 ; $8e02: c9 01
	beq loc_8e13             ; $8e04: f0 0d
loc_8e06:
	cmp #$02                 ; $8e06: c9 02

; === Block $8e03-$8e1b (Code) ===
	ora ($f0,x)              ; $8e03: 01 f0
	ora $02c9                ; $8e05: 0d c9 02
	bne loc_8e17             ; $8e08: d0 0d
loc_8e0a:
	jsr loc_ff74             ; $8e0a: 20 74 ff
loc_8e0d:
	jsr loc_ff74             ; $8e0d: 20 74 ff
loc_8e10:
	jsr loc_ff74             ; $8e10: 20 74 ff
loc_8e13:
	lda #$07                 ; $8e13: a9 07
	bne $8dfd                ; $8e15: d0 e6
loc_8e17:
	cmp #$0b                 ; $8e17: c9 0b
	beq loc_8e5f             ; $8e19: f0 44

; === Block $8e07-$8e08 (Code) ===
	*jam                     ; $8e07: 02

; === Block $8e0c-$8e15 (Code) ===
	*isc $7420,x             ; $8e0c: ff 20 74
	*isc $7420,x             ; $8e0f: ff 20 74
	*isc $07a9,x             ; $8e12: ff a9 07

; === Block $8e0e-$8e10 (Code) ===
	*nop $ff,x               ; $8e0e: 74 ff

; === Block $8e5f-$8e7a (Code) ===
loc_8e5f:
	lda #$0f                 ; $8e5f: a9 0f
	bne $8dfd                ; $8e61: d0 9a
loc_8e63:
	lda $3a                  ; $8e63: a5 3a
	lsr                      ; $8e65: 4a
	bcs loc_8e6f             ; $8e66: b0 07
loc_8e68:
	lda $3b                  ; $8e68: a5 3b
	lsr                      ; $8e6a: 4a
	bcc loc_8e74             ; $8e6b: 90 07
loc_8e6d:
	bcs loc_8e78             ; $8e6d: b0 09
loc_8e6f:
	lda $3b                  ; $8e6f: a5 3b
	lsr                      ; $8e71: 4a
	bcc loc_8e78             ; $8e72: 90 04
loc_8e74:
	lda #$1f                 ; $8e74: a9 1f
	bne $8dfd                ; $8e76: d0 85
loc_8e78:
	lda #$0f                 ; $8e78: a9 0f

; === Block $8e7c-$8eca (Code) ===
loc_8e7c:
	lda $45                  ; $8e7c: a5 45
	cmp #$01                 ; $8e7e: c9 01
	bne loc_8ed8             ; $8e80: d0 56
loc_8e82:
	lda $3b                  ; $8e82: a5 3b
	sta $3c                  ; $8e84: 85 3c
	lda #$0f                 ; $8e86: a9 0f
	sta $3e                  ; $8e88: 85 3e
	jsr loc_c1f0             ; $8e8a: 20 f0 c1
loc_8e8d:
	lda $3c                  ; $8e8d: a5 3c
	sta $42                  ; $8e8f: 85 42
	lda $3a                  ; $8e91: a5 3a
	sta $3c                  ; $8e93: 85 3c
	lda #$0f                 ; $8e95: a9 0f
	sta $3e                  ; $8e97: 85 3e
	jsr loc_c1f0             ; $8e99: 20 f0 c1
loc_8e9c:
	lda $42                  ; $8e9c: a5 42
	asl                      ; $8e9e: 0a
	asl                      ; $8e9f: 0a
	sta $3e                  ; $8ea0: 85 3e
	lda $3c                  ; $8ea2: a5 3c
	lsr                      ; $8ea4: 4a
	clc                      ; $8ea5: 18
	adc $3e                  ; $8ea6: 65 3e
	tax                      ; $8ea8: aa
	lda $f522,x              ; $8ea9: bd 22 f5
	sta $3e                  ; $8eac: 85 3e
	lda $3c                  ; $8eae: a5 3c
	lsr                      ; $8eb0: 4a
	bcs loc_8ebb             ; $8eb1: b0 08
loc_8eb3:
	lsr $3e                  ; $8eb3: 46 3e
	lsr $3e                  ; $8eb5: 46 3e
	lsr $3e                  ; $8eb7: 46 3e
	lsr $3e                  ; $8eb9: 46 3e
loc_8ebb:
	lda $3e                  ; $8ebb: a5 3e
	and #$0f                 ; $8ebd: 29 0f
	bne loc_8f04             ; $8ebf: d0 43
loc_8ec1:
	jsr loc_c55b             ; $8ec1: 20 5b c5
loc_8ec4:
	lda $e0                  ; $8ec4: a5 e0
	cmp #$02                 ; $8ec6: c9 02
	bne loc_8ed1             ; $8ec8: d0 07

; === Block $8eca-$8ed0 (Code) ===
	lda $95                  ; $8eca: a5 95
	and #$03                 ; $8ecc: 29 03
	beq loc_8f04             ; $8ece: f0 34
loc_8ed0:
	rts                      ; $8ed0: 60

; === Block $8ed1-$8ed7 (Code) ===
loc_8ed1:
	lda $95                  ; $8ed1: a5 95
	and #$01                 ; $8ed3: 29 01
	beq loc_8f04             ; $8ed5: f0 2d
loc_8ed7:
	rts                      ; $8ed7: 60

; === Block $8ed8-$8efa (Code) ===
loc_8ed8:
	cmp #$02                 ; $8ed8: c9 02
	bne loc_8ee0             ; $8eda: d0 04
loc_8edc:
	lda #$10                 ; $8edc: a9 10
	bne loc_8f04             ; $8ede: d0 24
loc_8ee0:
	cmp #$03                 ; $8ee0: c9 03
	bne loc_8ee8             ; $8ee2: d0 04
loc_8ee4:
	lda #$0d                 ; $8ee4: a9 0d
	bne loc_8f04             ; $8ee6: d0 1c
loc_8ee8:
	cmp #$06                 ; $8ee8: c9 06
	bne loc_8ef0             ; $8eea: d0 04
loc_8eec:
	lda #$12                 ; $8eec: a9 12
	bne loc_8f04             ; $8eee: d0 14
loc_8ef0:
	cmp #$1c                 ; $8ef0: c9 1c
	bcs $8efa                ; $8ef2: b0 06
loc_8ef4:
	lda $16                  ; $8ef4: a5 16
	cmp #$20                 ; $8ef6: c9 20
	beq $8efb                ; $8ef8: f0 01

; === Block $8ef9-$8f20 (Code) ===
	ora ($60,x)              ; $8ef9: 01 60
	lda $45                  ; $8efb: a5 45
	sec                      ; $8efd: 38
	sbc #$0f                 ; $8efe: e9 0f
	tax                      ; $8f00: aa
	lda $f542,x              ; $8f01: bd 42 f5
loc_8f04:
	sta $3e                  ; $8f04: 85 3e
	asl                      ; $8f06: 0a
	asl                      ; $8f07: 0a
	clc                      ; $8f08: 18
	adc $3e                  ; $8f09: 65 3e
	sta $3e                  ; $8f0b: 85 3e
	jsr loc_c55b             ; $8f0d: 20 5b c5
loc_8f10:
	lda $95                  ; $8f10: a5 95
	and #$07                 ; $8f12: 29 07
	cmp #$05                 ; $8f14: c9 05
	bcs $8f0d                ; $8f16: b0 f5
loc_8f18:
	adc $3e                  ; $8f18: 65 3e
	tax                      ; $8f1a: aa
	lda $f54f,x              ; $8f1b: bd 4f f5
	sta $3c                  ; $8f1e: 85 3c

; === Block $8efa-$8efa (Code) ===
	rts                      ; $8efa: 60

; === Block $8f7f-$8fac (Code) ===
	cmp $bea5,y              ; $8f7f: d9 a5 be
	lsr                      ; $8f82: 4a
	lsr                      ; $8f83: 4a
	lsr                      ; $8f84: 4a
	lsr                      ; $8f85: 4a
	lsr                      ; $8f86: 4a
	clc                      ; $8f87: 18
	adc #$09                 ; $8f88: 69 09
	sta $ab                  ; $8f8a: 85 ab
	lda $be                  ; $8f8c: a5 be
	lsr                      ; $8f8e: 4a
	lsr                      ; $8f8f: 4a
	and #$07                 ; $8f90: 29 07
	clc                      ; $8f92: 18
	adc #$11                 ; $8f93: 69 11
	sta $ac                  ; $8f95: 85 ac
	lda $be                  ; $8f97: a5 be
	and #$03                 ; $8f99: 29 03
	clc                      ; $8f9b: 18
	adc #$19                 ; $8f9c: 69 19
	sta $ad                  ; $8f9e: 85 ad
	jsr loc_c6f0             ; $8fa0: 20 f0 c6
loc_8fa3:
	ora ($20,x)              ; $8fa3: 01 20
	cpx $cf                  ; $8fa5: e4 cf
	lda #$01                 ; $8fa7: a9 01
	jsr loc_a7a2             ; $8fa9: 20 a2 a7
loc_8fac:
	jmp loc_cf6a             ; $8fac: 4c 6a cf

; === Block $914d-$9167 (Code) ===
	jsr loc_c7c5             ; $914d: 20 c5 c7
loc_9150:
	ora $4c,x                ; $9150: 15 4c
	cmp $20cf,y              ; $9152: d9 cf 20 PPUDATA
	*sre $a5c5,y             ; $9155: 5b c5 a5
	sta $4a,x                ; $9158: 95 4a
	bcc loc_9163             ; $915a: 90 07
loc_915c:
	jsr loc_c7c5             ; $915c: 20 c5 c7
loc_915f:
	*slo loc_d94c,x          ; $915f: 1f 4c d9
	*dcp $c520               ; $9162: cf 20 c5
	*dcp $20                 ; $9165: c7 20
	jmp loc_cfd9             ; $9167: 4c d9 cf

; === Block $9154-$915a (Code) ===
	jsr loc_c55b             ; $9154: 20 5b c5
loc_9157:
	lda $95                  ; $9157: a5 95
	lsr                      ; $9159: 4a

; === Block $915b-$915f (Code) ===
	*slo $20                 ; $915b: 07 20
	cmp $c7                  ; $915d: c5 c7

; === Block $915e-$9160 (Code) ===
	*dcp $1f                 ; $915e: c7 1f
	jmp loc_cfd9             ; $9160: 4c d9 cf

; === Block $9161-$9170 (Code) ===
	cmp $20cf,y              ; $9161: d9 cf 20 PPUDATA
	cmp $c7                  ; $9164: c5 c7
	jsr loc_d94c             ; $9166: 20 4c d9
loc_9169:
	*dcp $3cb1               ; $9169: cf b1 3c PPUMASK
	cmp #$07                 ; $916c: c9 07
	bcs loc_9173             ; $916e: b0 03
loc_9170:
	jmp loc_d553             ; $9170: 4c 53 d5

; === Block $9163-$9166 (Code) ===
loc_9163:
	jsr loc_c7c5             ; $9163: 20 c5 c7

; === Block $916a-$916c (Code) ===
	lda ($3c),y              ; $916a: b1 3c

; === Block $916f-$9177 (Code) ===
	*slo ($4c,x)             ; $916f: 03 4c
	*sre ($d5),y             ; $9171: 53 d5
loc_9173:
	cmp #$0c                 ; $9173: c9 0c
	bcs loc_917a             ; $9175: b0 03
loc_9177:
	jmp loc_d6a7             ; $9177: 4c a7 d6

; === Block $9174-$9177 (Code) ===
	*nop $03b0               ; $9174: 0c b0 03

; === Block $9178-$917e (Code) ===
	*lax $d6                 ; $9178: a7 d6
loc_917a:
	cmp #$0f                 ; $917a: c9 0f
	bcs loc_9181             ; $917c: b0 03

; === Block $9180-$918c (Code) ===
	*dcp $c9,x               ; $9180: d7 c9
	ora ($b0),y              ; $9182: 11 b0
	*slo ($4c,x)             ; $9184: 03 4c
	*sre ($d8,x)             ; $9186: 43 d8
	cmp #$16                 ; $9188: c9 16
	bcs loc_918f             ; $918a: b0 03
loc_918c:
	jmp loc_d895             ; $918c: 4c 95 d8

; === Block $9181-$9185 (Code) ===
loc_9181:
	cmp #$11                 ; $9181: c9 11
	bcs $9188                ; $9183: b0 03
loc_9185:
	jmp loc_d843             ; $9185: 4c 43 d8

; === Block $9189-$91ad (Code) ===
	asl $b0,x                ; $9189: 16 b0
	*slo ($4c,x)             ; $918b: 03 4c
	sta $d8,x                ; $918d: 95 d8
loc_918f:
	cmp #$5e                 ; $918f: c9 5e
	bcs loc_91c5             ; $9191: b0 32
loc_9193:
	pha                      ; $9193: 48
	lda $df                  ; $9194: a5 df
	and #$08                 ; $9196: 29 08
	bne loc_91b1             ; $9198: d0 17
loc_919a:
	pla                      ; $919a: 68
	cmp #$23                 ; $919b: c9 23
	bne loc_91a6             ; $919d: d0 07
loc_919f:
	jsr loc_c7c5             ; $919f: 20 c5 c7
loc_91a2:
	ora ($4c,x)              ; $91a2: 01 4c
	cmp $c9cf,y              ; $91a4: d9 cf c9
	bit $d0                  ; $91a7: 24 d0
	php                      ; $91a9: 08
	jsr loc_c7c5             ; $91aa: 20 c5 c7
loc_91ad:
	brk                      ; $91ad: 00

; === Block $918e-$918f (Code) ===
	cld                      ; $918e: d8

; === Block $9190-$9193 (Code) ===
	lsr $32b0,x              ; $9190: 5e b0 32 PPUCTRL

; === Block $9197-$9198 (Code) ===
	php                      ; $9197: 08

; === Block $9199-$919b (Code) ===
	*slo $68,x               ; $9199: 17 68

; === Block $91a1-$91a3 (Code) ===
	*dcp $01                 ; $91a1: c7 01
	jmp loc_cfd9             ; $91a3: 4c d9 cf

; === Block $91a5-$91aa (Code) ===
	*dcp $24c9               ; $91a5: cf c9 24 PPUMASK
	bne loc_91b2             ; $91a8: d0 08

; === Block $91a6-$91a8 (Code) ===
loc_91a6:
	cmp #$24                 ; $91a6: c9 24

; === Block $91ab-$91ad (Code) ===
	cmp $c7                  ; $91ab: c5 c7

; === Block $91af-$91d1 (Code) ===
	cmp $68cf,y              ; $91af: d9 cf 68
loc_91b2:
	pha                      ; $91b2: 48
	clc                      ; $91b3: 18
	adc #$2f                 ; $91b4: 69 2f
	jsr loc_c7bd             ; $91b6: 20 bd c7
loc_91b9:
	pla                      ; $91b9: 68
	cmp #$1a                 ; $91ba: c9 1a
	bne loc_91c2             ; $91bc: d0 04
loc_91be:
	jsr loc_c7cb             ; $91be: 20 cb c7
loc_91c1:
	bcs loc_920f             ; $91c1: b0 4c
loc_91c3:
	cmp $c9cf,y              ; $91c3: d9 cf c9
	*jam                     ; $91c6: 62
	bcs loc_91f4             ; $91c7: b0 2b
loc_91c9:
	clc                      ; $91c9: 18
	adc #$2f                 ; $91ca: 69 2f
	sta $de                  ; $91cc: 85 de
	jsr loc_c7bd             ; $91ce: 20 bd c7

; === Block $91b1-$91b2 (Code) ===
loc_91b1:
	pla                      ; $91b1: 68

; === Block $91b8-$91ba (Code) ===
	*dcp $68                 ; $91b8: c7 68

; === Block $91bb-$91bc (Code) ===
	*nop                     ; $91bb: 1a

; === Block $91bd-$91bf (Code) ===
	*nop $20                 ; $91bd: 04 20

; === Block $91c2-$91c2 (Code) ===
loc_91c2:
	jmp loc_cfd9             ; $91c2: 4c d9 cf

; === Block $91c4-$91c7 (Code) ===
	*dcp $62c9               ; $91c4: cf c9 62

; === Block $91c5-$91c7 (Code) ===
loc_91c5:
	cmp #$62                 ; $91c5: c9 62

; === Block $9205-$9245 (Code) ===
	*rra ($d0,x)             ; $9205: 63 d0
	asl                      ; $9207: 0a
	lda $df                  ; $9208: a5 df
	and #$03                 ; $920a: 29 03
	beq loc_91fc             ; $920c: f0 ee
loc_920e:
	lda #$9d                 ; $920e: a9 9d
	bne loc_9242             ; $9210: d0 30
loc_9212:
	cmp #$64                 ; $9212: c9 64
	bne loc_9224             ; $9214: d0 0e
loc_9216:
	lda $df                  ; $9216: a5 df
	and #$03                 ; $9218: 29 03
	bne loc_9220             ; $921a: d0 04
loc_921c:
	lda #$9e                 ; $921c: a9 9e
	bne loc_9242             ; $921e: d0 22
loc_9220:
	lda #$9f                 ; $9220: a9 9f
	bne loc_9242             ; $9222: d0 1e
loc_9224:
	cmp #$65                 ; $9224: c9 65
	bne loc_9248             ; $9226: d0 20
loc_9228:
	lda $df                  ; $9228: a5 df
	and #$03                 ; $922a: 29 03
	bne loc_9240             ; $922c: d0 12
loc_922e:
	jsr loc_c7cb             ; $922e: 20 cb c7
loc_9231:
	ldy #$20                 ; $9231: a0 20
	beq loc_91fb             ; $9233: f0 c6
loc_9235:
	ora #$f0                 ; $9235: 09 f0
	*nop $20                 ; $9237: 04 20
	*axs #$c7                ; $9239: cb c7
	lda ($a9,x)              ; $923b: a1 a9
	ldx #$d0                 ; $923d: a2 d0
	*jam                     ; $923f: 02
loc_9240:
	lda #$a3                 ; $9240: a9 a3
loc_9242:
	jsr loc_c7bd             ; $9242: 20 bd c7
loc_9245:
	jmp loc_cfd9             ; $9245: 4c d9 cf

; === Block $920f-$9212 (Code) ===
loc_920f:
	sta $30d0,x              ; $920f: 9d d0 30 PPUCTRL

; === Block $9211-$9218 (Code) ===
	bmi loc_91dc             ; $9211: 30 c9
loc_9213:
	*nop $d0                 ; $9213: 64 d0
	asl $dfa5                ; $9215: 0e a5 df

; === Block $9219-$9220 (Code) ===
	*slo ($d0,x)             ; $9219: 03 d0
	*nop $a9                 ; $921b: 04 a9
	*shx $22d0,y             ; $921d: 9e d0 22 PPUCTRL

; === Block $9248-$92c7 (Code) ===
loc_9248:
	cmp #$66                 ; $9248: c9 66
	bne loc_926a             ; $924a: d0 1e
loc_924c:
	lda #$0c                 ; $924c: a9 0c
	jsr loc_e055             ; $924e: 20 55 e0
loc_9251:
	cmp #$ff                 ; $9251: c9 ff
	bne loc_9266             ; $9253: d0 11
loc_9255:
	lda #$0e                 ; $9255: a9 0e
	jsr loc_e055             ; $9257: 20 55 e0
loc_925a:
	cmp #$ff                 ; $925a: c9 ff
	bne loc_9266             ; $925c: d0 08
loc_925e:
	jsr loc_c7cb             ; $925e: 20 cb c7
loc_9261:
	ldy $a9                  ; $9261: a4 a9
	dec $d0                  ; $9263: c6 d0
	*nop $a5a9,x             ; $9265: dc a9 a5
	bne loc_9242             ; $9268: d0 d8
loc_926a:
	cmp #$67                 ; $926a: c9 67
	bne loc_9298             ; $926c: d0 2a
loc_926e:
	lda $cf                  ; $926e: a5 cf
	and #$c0                 ; $9270: 29 c0
	bne loc_9278             ; $9272: d0 04
loc_9274:
	lda #$a6                 ; $9274: a9 a6
	bne loc_9242             ; $9276: d0 ca
loc_9278:
	jsr loc_c7cb             ; $9278: 20 cb c7
loc_927b:
	*lax $a5                 ; $927b: a7 a5
	*dcp $0510               ; $927d: cf 10 05
	lda #$0b                 ; $9280: a9 0b
	jsr $e04b                ; $9282: 20 4b e0
loc_9285:
	bit $cf                  ; $9285: 24 cf
	bvc loc_928e             ; $9287: 50 05
loc_9289:
	lda #$09                 ; $9289: a9 09
	jsr $e04b                ; $928b: 20 4b e0
loc_928e:
	lda $cf                  ; $928e: a5 cf
	and #$3f                 ; $9290: 29 3f
	sta $cf                  ; $9292: 85 cf
	lda #$a8                 ; $9294: a9 a8
	bne loc_9242             ; $9296: d0 aa
loc_9298:
	cmp #$68                 ; $9298: c9 68
	bne loc_92ac             ; $929a: d0 10
loc_929c:
	lda $be                  ; $929c: a5 be
	and #$e0                 ; $929e: 29 e0
	cmp #$e0                 ; $92a0: c9 e0
	beq loc_92a8             ; $92a2: f0 04
loc_92a4:
	lda #$a9                 ; $92a4: a9 a9
	bne loc_9242             ; $92a6: d0 9a
loc_92a8:
	lda #$aa                 ; $92a8: a9 aa
	bne loc_9242             ; $92aa: d0 96
loc_92ac:
	cmp #$69                 ; $92ac: c9 69
	bne loc_92cf             ; $92ae: d0 1f
loc_92b0:
	lda #$06                 ; $92b0: a9 06
	jsr loc_e055             ; $92b2: 20 55 e0
loc_92b5:
	cmp #$ff                 ; $92b5: c9 ff
	bne loc_92bf             ; $92b7: d0 06
loc_92b9:
	lda $cf                  ; $92b9: a5 cf
	and #$df                 ; $92bb: 29 df
	sta $cf                  ; $92bd: 85 cf
loc_92bf:
	lda $cf                  ; $92bf: a5 cf
	and #$20                 ; $92c1: 29 20
	bne loc_92ca             ; $92c3: d0 05
loc_92c5:
	lda #$ac                 ; $92c5: a9 ac
	jmp loc_d242             ; $92c7: 4c 42 d2

; === Block $9266-$9268 (Code) ===
loc_9266:
	lda #$a5                 ; $9266: a9 a5

; === Block $92ca-$92cc (Code) ===
loc_92ca:
	lda #$ab                 ; $92ca: a9 ab
	jmp loc_d242             ; $92cc: 4c 42 d2

; === Block $92cf-$92e1 (Code) ===
loc_92cf:
	cmp #$6a                 ; $92cf: c9 6a
	bne loc_92e5             ; $92d1: d0 12
loc_92d3:
	jsr loc_c7cb             ; $92d3: 20 cb c7
loc_92d6:
	lda $3720                ; $92d6: ad 20 37 PPUCTRL
	*dcp $cba5,y             ; $92d9: db a5 cb
	sta $c6                  ; $92dc: 85 c6
	jsr loc_c6f0             ; $92de: 20 f0 c6
loc_92e1:
	brk                      ; $92e1: 00

; === Block $92e5-$92ec (Code) ===
loc_92e5:
	cmp #$6b                 ; $92e5: c9 6b
	bne loc_92f8             ; $92e7: d0 0f
loc_92e9:
	jsr loc_c7cb             ; $92e9: 20 cb c7
loc_92ec:
	jmp loc_cb20             ; $92ec: 4c 20 cb

; === Block $92f8-$930c (Code) ===
loc_92f8:
	cmp #$6c                 ; $92f8: c9 6c
	bne loc_9345             ; $92fa: d0 49
loc_92fc:
	lda #$0e                 ; $92fc: a9 0e
	jsr loc_e055             ; $92fe: 20 55 e0
loc_9301:
	cmp #$ff                 ; $9301: c9 ff
	bne loc_930e             ; $9303: d0 09
loc_9305:
	lda #$0d                 ; $9305: a9 0d
	jsr loc_e055             ; $9307: 20 55 e0
loc_930a:
	cmp #$ff                 ; $930a: c9 ff

; === Block $9345-$9362 (Code) ===
loc_9345:
	cmp #$6d                 ; $9345: c9 6d
	bne loc_93ab             ; $9347: d0 62
loc_9349:
	lda #$0e                 ; $9349: a9 0e
	jsr loc_e055             ; $934b: 20 55 e0
loc_934e:
	cmp #$ff                 ; $934e: c9 ff
	bne loc_930e             ; $9350: d0 bc
loc_9352:
	lda #$07                 ; $9352: a9 07
	jsr loc_e055             ; $9354: 20 55 e0
loc_9357:
	cmp #$ff                 ; $9357: c9 ff
	bne loc_9365             ; $9359: d0 0a
loc_935b:
	jsr loc_c7cb             ; $935b: 20 cb c7
loc_935e:
	*lax ($20),y             ; $935e: b3 20
	*rla $db,x               ; $9360: 37 db
	jmp loc_b228             ; $9362: 4c 28 b2

; === Block $9365-$938d (Code) ===
loc_9365:
	lda #$0c                 ; $9365: a9 0c
	jsr loc_e055             ; $9367: 20 55 e0
loc_936a:
	cmp #$ff                 ; $936a: c9 ff
	beq loc_93a2             ; $936c: f0 34
loc_936e:
	lda #$0d                 ; $936e: a9 0d
	jsr loc_e055             ; $9370: 20 55 e0
loc_9373:
	cmp #$ff                 ; $9373: c9 ff
	beq loc_93a2             ; $9375: f0 2b
loc_9377:
	jsr loc_c7cb             ; $9377: 20 cb c7
loc_937a:
	ldy $a9,x                ; $937a: b4 a9
	*nop $4b20               ; $937c: 0c 20 4b
	cpx #$a9                 ; $937f: e0 a9
	ora $4b20                ; $9381: 0d 20 4b
	cpx #$a9                 ; $9384: e0 a9
	asl $1b20                ; $9386: 0e 20 1b
	cpx #$20                 ; $9389: e0 20
	*nop $ff,x               ; $938b: 74 ff

; === Block $937e-$939f (Code) ===
loc_937e:
	*alr #$e0                ; $937e: 4b e0
	lda #$0d                 ; $9380: a9 0d
	jsr $e04b                ; $9382: 20 4b e0
loc_9385:
	lda #$0e                 ; $9385: a9 0e
	jsr loc_e01b             ; $9387: 20 1b e0
loc_938a:
	jsr loc_ff74             ; $938a: 20 74 ff
loc_938d:
	lda #$19                 ; $938d: a9 19
	sta $2001                ; $938f: 8d 01 20 PPUMASK
	ldx #$1e                 ; $9392: a2 1e
	jsr loc_ff74             ; $9394: 20 74 ff
loc_9397:
	dex                      ; $9397: ca
	bne $9394                ; $9398: d0 fa
loc_939a:
	lda #$18                 ; $939a: a9 18
	sta $2001                ; $939c: 8d 01 20 PPUMASK
	jmp loc_cfd9             ; $939f: 4c d9 cf

; === Block $93a2-$93af (Code) ===
loc_93a2:
	jsr loc_c7cb             ; $93a2: 20 cb c7
loc_93a5:
	eor #$a9                 ; $93a5: 49 a9
	ldx $104c                ; $93a7: ae 4c 10
	*dcp ($c9),y             ; $93aa: d3 c9
	ror $03f0                ; $93ac: 6e f0 03

; === Block $93a9-$93af (Code) ===
loc_93a9:
	bpl loc_937e             ; $93a9: 10 d3
loc_93ab:
	cmp #$6e                 ; $93ab: c9 6e
	beq loc_93b2             ; $93ad: f0 03
loc_93af:
	jmp loc_d464             ; $93af: 4c 64 d4

; === Block $93b2-$93c3 (Code) ===
loc_93b2:
	lda $df                  ; $93b2: a5 df
	and #$01                 ; $93b4: 29 01
	beq loc_9410             ; $93b6: f0 58
loc_93b8:
	jsr loc_c7cb             ; $93b8: 20 cb c7
loc_93bb:
	lda $08a9,y              ; $93bb: b9 a9 08
	jsr loc_e01b             ; $93be: 20 1b e0
loc_93c1:
	cpx #$04                 ; $93c1: e0 04

; === Block $93c3-$93cd (Code) ===
	bne loc_93ef             ; $93c3: d0 2a
loc_93c5:
	ldx #$00                 ; $93c5: a2 00
	lda $d54c,x              ; $93c7: bd 4c d5
	jsr loc_e055             ; $93ca: 20 55 e0

; === Block $93c4-$93c5 (Code) ===
	rol                      ; $93c4: 2a

; === Block $93c6-$93c6 (Code) ===
	brk                      ; $93c6: 00

; === Block $93c9-$93cd (Code) ===
	cmp $20,x                ; $93c9: d5 20
	eor $e0,x                ; $93cb: 55 e0

; === Block $93ef-$940d (Code) ===
loc_93ef:
	jsr loc_c7cb             ; $93ef: 20 cb c7
loc_93f2:
	*las loc_cb20,y          ; $93f2: bb 20 cb
	*dcp $bc                 ; $93f5: c7 bc
	lda $df                  ; $93f7: a5 df
	and #$fc                 ; $93f9: 29 fc
	ora #$02                 ; $93fb: 09 02
	sta $df                  ; $93fd: 85 df
	lda #$c6                 ; $93ff: a9 c6
	sta $78                  ; $9401: 85 78
	lda #$43                 ; $9403: a9 43
	sta $79                  ; $9405: 85 79
	jsr loc_ff74             ; $9407: 20 74 ff
loc_940a:
	jsr loc_b6da             ; $940a: 20 da b6
loc_940d:
	jmp loc_d433             ; $940d: 4c 33 d4

; === Block $9410-$9414 (Code) ===
loc_9410:
	lda $df                  ; $9410: a5 df
	and #$08                 ; $9412: 29 08

; === Block $9413-$9418 (Code) ===
loc_9413:
	php                      ; $9413: 08
	bne loc_941b             ; $9414: d0 05
loc_9416:
	lda #$bf                 ; $9416: a9 bf
	jmp loc_d242             ; $9418: 4c 42 d2

; === Block $941b-$9429 (Code) ===
loc_941b:
	jsr loc_c7cb             ; $941b: 20 cb c7
loc_941e:
	cpy #$a5                 ; $941e: c0 a5
	*dcp $c9                 ; $9420: c7 c9
	asl $07d0,x              ; $9422: 1e d0 07
	jsr loc_c7cb             ; $9425: 20 cb c7
loc_9428:
	*jam                     ; $9428: 02
	jmp loc_d433             ; $9429: 4c 33 d4

; === Block $9441-$944f (Code) ===
	sbc ($20),y              ; $9441: f1 20
	cmp $c7                  ; $9443: c5 c7
	bit $20                  ; $9445: 24 20
	cmp $c7                  ; $9447: c5 c7
	and $20                  ; $9449: 25 20
	beq loc_9413             ; $944b: f0 c6
loc_944d:
	ora #$c9                 ; $944d: 09 c9
	brk                      ; $944f: 00

; === Block $9442-$9445 (Code) ===
	jsr loc_c7c5             ; $9442: 20 c5 c7

; === Block $9444-$9449 (Code) ===
	*dcp $24                 ; $9444: c7 24
	jsr loc_c7c5             ; $9446: 20 c5 c7

; === Block $9448-$944d (Code) ===
	*dcp $25                 ; $9448: c7 25
	jsr loc_c6f0             ; $944a: 20 f0 c6

; === Block $944c-$9450 (Code) ===
	dec $09                  ; $944c: c6 09
	cmp #$00                 ; $944e: c9 00

; === Block $9494-$9496 (Code) ===
	lda #$16                 ; $9494: a9 16
	brk                      ; $9496: 00

; === Block $9495-$9499 (Code) ===
	asl $00,x                ; $9495: 16 00
	*nop $17                 ; $9497: 04 17
	brk                      ; $9499: 00

; === Block $9498-$949e (Code) ===
	*slo $00,x               ; $9498: 17 00
	*slo ($17,x)             ; $949a: 03 17
	lda #$02                 ; $949c: a9 02
	brk                      ; $949e: 00

; === Block $94b8-$94bd (Code) ===
	beq loc_9480             ; $94b8: f0 c6
loc_94ba:
	ora #$d0                 ; $94ba: 09 d0
	asl                      ; $94bc: 0a

; === Block $94c0-$94e4 (Code) ===
	asl $20,x                ; $94c0: 16 20
	beq loc_948a             ; $94c2: f0 c6
loc_94c4:
	ora #$f0                 ; $94c4: 09 f0
	jsr loc_cb20             ; $94c6: 20 20 cb
loc_94c9:
	*dcp $c9                 ; $94c9: c7 c9
	ldx #$28                 ; $94cb: a2 28
	jsr loc_ff74             ; $94cd: 20 74 ff
loc_94d0:
	dex                      ; $94d0: ca
	bne $94cd                ; $94d1: d0 fa
loc_94d3:
	lda #$02                 ; $94d3: a9 02
	jsr loc_a7a2             ; $94d5: 20 a2 a7
loc_94d8:
	lda #$03                 ; $94d8: a9 03
	jsr loc_a7a2             ; $94da: 20 a2 a7
loc_94dd:
	lda #$00                 ; $94dd: a9 00
	jsr loc_a7a2             ; $94df: 20 a2 a7
loc_94e2:
	lda #$26                 ; $94e2: a9 26

; === Block $94c8-$94cd (Code) ===
	*axs #$c7                ; $94c8: cb c7
	cmp #$a2                 ; $94ca: c9 a2
	plp                      ; $94cc: 28

; === Block $94d2-$94d3 (Code) ===
	*nop                     ; $94d2: fa

; === Block $968c-$969a (Code) ===
loc_968c:
	lda loc_9991,x           ; $968c: bd 91 99
	clc                      ; $968f: 18
	adc #$02                 ; $9690: 69 02
	sta $00a3,y              ; $9692: 99 a3 00
	cmp #$ff                 ; $9695: c9 ff
	beq loc_969d             ; $9697: f0 04
loc_9699:
	inx                      ; $9699: e8

; === Block $969a-$96a6 (Code) ===
	iny                      ; $969a: c8
	bne loc_968c             ; $969b: d0 ef
loc_969d:
	jsr loc_c6f0             ; $969d: 20 f0 c6
loc_96a0:
	php                      ; $96a0: 08
	pla                      ; $96a1: 68
	clc                      ; $96a2: 18
	adc $d7                  ; $96a3: 65 d7
	tax                      ; $96a5: aa
	rts                      ; $96a6: 60

; === Block $969c-$96a1 (Code) ===
	*isc $f020               ; $969c: ef 20 f0
	dec $08                  ; $969f: c6 08

; === Block $96b8-$96bd (Code) ===
	and $20d7,y              ; $96b8: 39 d7 20 PPUDATA
	*axs #$c7                ; $96bb: cb c7

; === Block $9717-$971c (Code) ===
	*axs #$c7                ; $9717: cb c7
	*slo $f020,x             ; $9719: 1f 20 f0

; === Block $9718-$971a (Code) ===
	*dcp $1f                 ; $9718: c7 1f

; === Block $972b-$9739 (Code) ===
	jsr loc_e01b             ; $972b: 20 1b e0
loc_972e:
	cpx #$04                 ; $972e: e0 04
	bne loc_9706             ; $9730: d0 d4
loc_9732:
	jsr loc_c7cb             ; $9732: 20 cb c7
loc_9735:
	and ($4c,x)              ; $9735: 21 4c
	asl $d7,x                ; $9737: 16 d7

; === Block $972c-$9735 (Code) ===
	*slo $e0e0,y             ; $972c: 1b e0 e0
	*nop $d0                 ; $972f: 04 d0
	*nop $20,x               ; $9731: d4 20
	*axs #$c7                ; $9733: cb c7

; === Block $9738-$973a (Code) ===
	*dcp $20,x               ; $9738: d7 20

; === Block $97c3-$97df (Code) ===
	ldy #$d7                 ; $97c3: a0 d7
	cmp #$0c                 ; $97c5: c9 0c
	bne loc_97da             ; $97c7: d0 11
loc_97c9:
	pha                      ; $97c9: 48
	bit $cf                  ; $97ca: 24 cf
	bvc loc_97e3             ; $97cc: 50 15
loc_97ce:
	pla                      ; $97ce: 68
	jsr loc_c7cb             ; $97cf: 20 cb c7
loc_97d2:
	clc                      ; $97d2: 18
	jsr loc_c7cb             ; $97d3: 20 cb c7
loc_97d6:
	*slo $4c,x               ; $97d6: 17 4c
	*nop                     ; $97d8: 7a
	*dcp $c9,x               ; $97d9: d7 c9
	asl $06d0                ; $97db: 0e d0 06
	pha                      ; $97de: 48

; === Block $97c6-$97c9 (Code) ===
	*nop $11d0               ; $97c6: 0c d0 11

; === Block $97c8-$97ca (Code) ===
	ora ($48),y              ; $97c8: 11 48

; === Block $97da-$97de (Code) ===
loc_97da:
	cmp #$0e                 ; $97da: c9 0e
	bne loc_97e4             ; $97dc: d0 06

; === Block $985d-$9860 (Code) ===
	ldy $e538,x              ; $985d: bc 38 e5
	brk                      ; $9860: 00

; === Block $985f-$986f (Code) ===
	sbc $00                  ; $985f: e5 00
	sta $3c                  ; $9861: 85 3c
	lda $bd                  ; $9863: a5 bd
	sbc $01                  ; $9865: e5 01
	sta $3d                  ; $9867: 85 3d
	bcs loc_9872             ; $9869: b0 07
loc_986b:
	jsr loc_c7cb             ; $986b: 20 cb c7
loc_986e:
	*jam                     ; $986e: 22
	jmp loc_d855             ; $986f: 4c 55 d8

; === Block $9862-$9865 (Code) ===
	*nop $bda5,x             ; $9862: 3c a5 bd

; === Block $9868-$986b (Code) ===
	and $07b0,x              ; $9868: 3d b0 07

; === Block $9871-$988d (Code) ===
	cld                      ; $9871: d8
loc_9872:
	lda #$02                 ; $9872: a9 02
	jsr loc_e01b             ; $9874: 20 1b e0
loc_9877:
	cpx #$04                 ; $9877: e0 04
	bne loc_9882             ; $9879: d0 07
loc_987b:
	jsr loc_c7cb             ; $987b: 20 cb c7
loc_987e:
	and ($4c,x)              ; $987e: 21 4c
	eor $d8,x                ; $9880: 55 d8
loc_9882:
	lda $3c                  ; $9882: a5 3c
	sta $bc                  ; $9884: 85 bc
	lda $3d                  ; $9886: a5 3d
	sta $bd                  ; $9888: 85 bd
	jsr loc_c6f0             ; $988a: 20 f0 c6
loc_988d:
	brk                      ; $988d: 00

; === Block $987a-$987e (Code) ===
	*slo $20                 ; $987a: 07 20
	*axs #$c7                ; $987c: cb c7

; === Block $987d-$987f (Code) ===
	*dcp $21                 ; $987d: c7 21
	jmp loc_d855             ; $987f: 4c 55 d8

; === Block $9883-$9886 (Code) ===
	*nop $bc85,x             ; $9883: 3c 85 bc

; === Block $9889-$989e (Code) ===
	lda $f020,x              ; $9889: bd 20 f0
	dec $00                  ; $988c: c6 00
	jsr loc_c7cb             ; $988e: 20 cb c7
loc_9891:
	bpl loc_98df             ; $9891: 10 4c
loc_9893:
	*sre $38d8               ; $9893: 4f d8 38 PPUCTRL
	sbc #$11                 ; $9896: e9 11
	tax                      ; $9898: aa
	lda $998c,x              ; $9899: bd 8c 99
	sta $00                  ; $989c: 85 00

; === Block $98b5-$98c6 (Code) ===
loc_98b5:
	sec                      ; $98b5: 38
	sbc $00                  ; $98b6: e5 00
	sta $3c                  ; $98b8: 85 3c
	lda $bd                  ; $98ba: a5 bd
	sbc $01                  ; $98bc: e5 01
	sta $3d                  ; $98be: 85 3d
	bcs $98c9                ; $98c0: b0 07
loc_98c2:
	jsr loc_c7cb             ; $98c2: 20 cb c7
loc_98c5:
	*jam                     ; $98c5: 22

; === Block $98c4-$98c6 (Code) ===
	*dcp $22                 ; $98c4: c7 22
	jmp loc_d8af             ; $98c6: 4c af d8

; === Block $98c7-$98d4 (Code) ===
	*lax $a5d8               ; $98c7: af d8 a5
	*nop $bc85,x             ; $98ca: 3c 85 bc
	lda $3d                  ; $98cd: a5 3d
	sta $bd                  ; $98cf: 85 bd
	jsr loc_c6f0             ; $98d1: 20 f0 c6
loc_98d4:
	brk                      ; $98d4: 00

; === Block $98c9-$98cd (Code) ===
	lda $3c                  ; $98c9: a5 3c
	sta $bc                  ; $98cb: 85 bc

; === Block $98cc-$98cf (Code) ===
	ldy $3da5,x              ; $98cc: bc a5 3d PPUSCROLL

; === Block $98ce-$98d1 (Code) ===
	and $bd85,x              ; $98ce: 3d 85 bd

; === Block $98d8-$98e1 (Code) ===
	ora #$20                 ; $98d8: 09 20
	ora $d9,x                ; $98da: 15 d9
	jsr loc_c212             ; $98dc: 20 12 c2
loc_98df:
	lda #$15                 ; $98df: a9 15
	brk                      ; $98e1: 00

; === Block $98e3-$98ef (Code) ===
	*slo $a5,x               ; $98e3: 17 a5
	dex                      ; $98e5: ca
	sta $c5                  ; $98e6: 85 c5
	lda $cb                  ; $98e8: a5 cb
	sta $c6                  ; $98ea: 85 c6
	jsr loc_c6f0             ; $98ec: 20 f0 c6
loc_98ef:
	brk                      ; $98ef: 00

; === Block $98e4-$98e6 (Code) ===
	lda $ca                  ; $98e4: a5 ca

; === Block $98e9-$98ef (Code) ===
	*axs #$85                ; $98e9: cb 85
	dec $20                  ; $98eb: c6 20
	beq loc_98b5             ; $98ed: f0 c6

; === Block $98f1-$98f3 (Code) ===
	ora $d9,x                ; $98f1: 15 d9
	brk                      ; $98f3: 00

; === Block $98f7-$9916 (Code) ===
	*nop $00                 ; $98f7: 04 00
	*nop $17                 ; $98f9: 04 17
	jsr loc_c529             ; $98fb: 20 29 c5
loc_98fe:
	lda $df                  ; $98fe: a5 df
	lsr                      ; $9900: 4a
	bcc loc_990a             ; $9901: 90 07
loc_9903:
	jsr loc_c7cb             ; $9903: 20 cb c7
loc_9906:
	asl $4c                  ; $9906: 06 4c
	asl $20d9                ; $9908: 0e d9 20 PPUMASK
	*axs #$c7                ; $990b: cb c7
	php                      ; $990d: 08
	jsr loc_c7cb             ; $990e: 20 cb c7
loc_9911:
	*slo $4c                 ; $9911: 07 4c
	cmp $adcf,y              ; $9913: d9 cf ad

; === Block $98ff-$9906 (Code) ===
	*dcp $904a,x             ; $98ff: df 4a 90
	*slo $20                 ; $9902: 07 20
	*axs #$c7                ; $9904: cb c7

; === Block $990a-$990d (Code) ===
loc_990a:
	jsr loc_c7cb             ; $990a: 20 cb c7

; === Block $9983-$9999 (Code) ===
	sta $602f                ; $9983: 8d 2f 60
	lda #$00                 ; $9986: a9 00
	sta $91                  ; $9988: 85 91
	sta $93                  ; $998a: 85 93
	ldx #$04                 ; $998c: a2 04
	asl $90                  ; $998e: 06 90
	rol $91                  ; $9990: 26 91
	asl $92                  ; $9992: 06 92
	rol $93                  ; $9994: 26 93
	dex                      ; $9996: ca
	bne $998e                ; $9997: d0 f5
loc_9999:
	jmp loc_b097             ; $9999: 4c 97 b0

; === Block $9984-$9987 (Code) ===
	*rla $a960               ; $9984: 2f 60 a9
	brk                      ; $9987: 00

; === Block $998b-$9994 (Code) ===
	*sha ($a2),y             ; $998b: 93 a2
	*nop $06                 ; $998d: 04 06
	bcc loc_99b7             ; $998f: 90 26
loc_9991:
	sta ($06),y              ; $9991: 91 06
	*jam                     ; $9993: 92

; === Block $99b7-$99c0 (Code) ===
loc_99b7:
	jsr loc_ac17             ; $99b7: 20 17 ac
loc_99ba:
	lda $3c                  ; $99ba: a5 3c
	cmp #$05                 ; $99bc: c9 05
	bne loc_99c3             ; $99be: d0 03
loc_99c0:
	jmp loc_d93d             ; $99c0: 4c 3d d9

; === Block $99c3-$99fb (Code) ===
loc_99c3:
	cmp #$03                 ; $99c3: c9 03
	bne loc_9a06             ; $99c5: d0 3f
loc_99c7:
	ldx #$00                 ; $99c7: a2 00
	ldy #$00                 ; $99c9: a0 00
	lda $45                  ; $99cb: a5 45
	cmp $f461,x              ; $99cd: dd 61 f4
	bne loc_99fe             ; $99d0: d0 2c
loc_99d2:
	lda $3a                  ; $99d2: a5 3a
	cmp $f462,x              ; $99d4: dd 62 f4
	bne loc_99fe             ; $99d7: d0 25
loc_99d9:
	lda $3b                  ; $99d9: a5 3b
	cmp $f463,x              ; $99db: dd 63 f4
	bne loc_99fe             ; $99de: d0 1e
loc_99e0:
	lda #$03                 ; $99e0: a9 03
	pha                      ; $99e2: 48
	lda $f3c8,x              ; $99e3: bd c8 f3
	sta $45                  ; $99e6: 85 45
	lda $f3c9,x              ; $99e8: bd c9 f3
	sta $3a                  ; $99eb: 85 3a
	sta $8e                  ; $99ed: 85 8e
	sta $90                  ; $99ef: 85 90
	lda $f3ca,x              ; $99f1: bd ca f3
	sta $3b                  ; $99f4: 85 3b
	sta $8f                  ; $99f6: 85 8f
	sta $92                  ; $99f8: 85 92
	pla                      ; $99fa: 68
	jmp loc_d981             ; $99fb: 4c 81 d9

; === Block $99fe-$9a06 (Code) ===
loc_99fe:
	inx                      ; $99fe: e8
	inx                      ; $99ff: e8
	inx                      ; $9a00: e8
	iny                      ; $9a01: c8
	cpx #$99                 ; $9a02: e0 99
	bne $99cb                ; $9a04: d0 c5

; === Block $9a06-$9a10 (Code) ===
loc_9a06:
	jsr loc_c6f0             ; $9a06: 20 f0 c6
loc_9a09:
	*jam                     ; $9a09: 02
	jsr loc_c7cb             ; $9a0a: 20 cb c7
loc_9a0d:
	ora loc_d94c             ; $9a0d: 0d 4c d9

; === Block $9a12-$9a31 (Code) ===
	dec $3e85                ; $9a12: ce 85 3e PPUSCROLL
	lda $cf                  ; $9a15: a5 cf
	and #$03                 ; $9a17: 29 03
	sta $3f                  ; $9a19: 85 3f
	ora $3e                  ; $9a1b: 05 3e
	bne loc_9a2a             ; $9a1d: d0 0b
loc_9a1f:
	jsr loc_c6f0             ; $9a1f: 20 f0 c6
loc_9a22:
	*jam                     ; $9a22: 02
	jsr loc_c7cb             ; $9a23: 20 cb c7
loc_9a26:
	and ($4c),y              ; $9a26: 31 4c
	cmp $20cf,y              ; $9a28: d9 cf 20 PPUDATA
	lsr $db,x                ; $9a2b: 56 db
	cmp #$ff                 ; $9a2d: c9 ff
	bne loc_9a34             ; $9a2f: d0 03
loc_9a31:
	jmp loc_cf6a             ; $9a31: 4c 6a cf

; === Block $9a14-$9a17 (Code) ===
	rol $cfa5,x              ; $9a14: 3e a5 cf

; === Block $9a1a-$9a1d (Code) ===
	*rla $3e05,x             ; $9a1a: 3f 05 3e PPUSCROLL

; === Block $9a2a-$9a2d (Code) ===
loc_9a2a:
	jsr loc_db56             ; $9a2a: 20 56 db

; === Block $9a34-$9a3a (Code) ===
loc_9a34:
	pha                      ; $9a34: 48
	jsr loc_c6f0             ; $9a35: 20 f0 c6
loc_9a38:
	*jam                     ; $9a38: 02
	pla                      ; $9a39: 68

; === Block $9a3a-$9a44 (Code) ===
	jsr loc_db85             ; $9a3a: 20 85 db
loc_9a3d:
	cmp #$32                 ; $9a3d: c9 32
	bne loc_9a47             ; $9a3f: d0 06
loc_9a41:
	jsr loc_c7bd             ; $9a41: 20 bd c7
loc_9a44:
	jmp loc_cfd9             ; $9a44: 4c d9 cf

; === Block $9a3c-$9a3f (Code) ===
	*dcp $32c9,y             ; $9a3c: db c9 32 PPUMASK

; === Block $9a47-$9a4e (Code) ===
loc_9a47:
	cmp #$00                 ; $9a47: c9 00
	bne loc_9a51             ; $9a49: d0 06
loc_9a4b:
	jsr loc_dbb8             ; $9a4b: 20 b8 db

; === Block $9a4e-$9a4e (Code) ===
	jmp loc_cfd9             ; $9a4e: 4c d9 cf

; === Block $9a50-$9a6e (Code) ===
	*dcp $01c9               ; $9a50: cf c9 01
	bne loc_9a5c             ; $9a53: d0 07
loc_9a55:
	jsr loc_c7cb             ; $9a55: 20 cb c7
loc_9a58:
	*rla ($4c),y             ; $9a58: 33 4c
	cmp $c9cf,y              ; $9a5a: d9 cf c9
	*jam                     ; $9a5d: 02
	beq loc_9a55             ; $9a5e: f0 f5
loc_9a60:
	cmp #$03                 ; $9a60: c9 03
	bne loc_9a94             ; $9a62: d0 30
loc_9a64:
	lda $16                  ; $9a64: a5 16
	cmp #$20                 ; $9a66: c9 20
	bne loc_9a55             ; $9a68: d0 eb
loc_9a6a:
	lda #$50                 ; $9a6a: a9 50
	sta $da                  ; $9a6c: 85 da

; === Block $9a51-$9a53 (Code) ===
loc_9a51:
	cmp #$01                 ; $9a51: c9 01

; === Block $9a5c-$9a5e (Code) ===
loc_9a5c:
	cmp #$02                 ; $9a5c: c9 02

; === Block $9a8c-$9a91 (Code) ===
	*nop $17                 ; $9a8c: 04 17
	jsr loc_b30e             ; $9a8e: 20 0e b3
loc_9a91:
	jmp loc_da7d             ; $9a91: 4c 7d da

; === Block $9a8d-$9a9c (Code) ===
	*slo $20,x               ; $9a8d: 17 20
	asl $4cb3                ; $9a8f: 0e b3 4c
	adc $c9da,x              ; $9a92: 7d da c9
	*slo $d0                 ; $9a95: 07 d0
	*slo $a9                 ; $9a97: 07 a9
	*isc loc_db85,x          ; $9a99: ff 85 db
	jmp loc_cfd9             ; $9a9c: 4c d9 cf

; === Block $9a90-$9a92 (Code) ===
	*lax ($4c),y             ; $9a90: b3 4c

; === Block $9a94-$9a9c (Code) ===
loc_9a94:
	cmp #$07                 ; $9a94: c9 07
	bne loc_9a9f             ; $9a96: d0 07
loc_9a98:
	lda #$ff                 ; $9a98: a9 ff
	sta $db                  ; $9a9a: 85 db

; === Block $9a9f-$9aab (Code) ===
loc_9a9f:
	cmp #$05                 ; $9a9f: c9 05
	bne loc_9ae3             ; $9aa1: d0 40
loc_9aa3:
	lda $45                  ; $9aa3: a5 45
	cmp #$1c                 ; $9aa5: c9 1c
	bcc $9ab0                ; $9aa7: 90 07
loc_9aa9:
	ldx #$27                 ; $9aa9: a2 27

; === Block $9aab-$9aad (Code) ===
	lda #$02                 ; $9aab: a9 02
	jmp loc_d9e2             ; $9aad: 4c e2 d9

; === Block $9aac-$9aad (Code) ===
	*jam                     ; $9aac: 02

; === Block $9aae-$9ab8 (Code) ===
	*nop #$d9                ; $9aae: e2 d9
	cmp #$18                 ; $9ab0: c9 18
	bcc loc_9abb             ; $9ab2: 90 07
loc_9ab4:
	ldx #$39                 ; $9ab4: a2 39
	lda #$02                 ; $9ab6: a9 02
	jmp loc_d9e2             ; $9ab8: 4c e2 d9

; === Block $9aaf-$9ab2 (Code) ===
	cmp $18c9,y              ; $9aaf: d9 c9 18

; === Block $9ab1-$9ab2 (Code) ===
	clc                      ; $9ab1: 18

; === Block $9abb-$9ac3 (Code) ===
loc_9abb:
	cmp #$16                 ; $9abb: c9 16
	bcc loc_9ac6             ; $9abd: 90 07
loc_9abf:
	ldx #$18                 ; $9abf: a2 18
	lda #$02                 ; $9ac1: a9 02
	jmp loc_d9e2             ; $9ac3: 4c e2 d9

; === Block $9ac6-$9aca (Code) ===
loc_9ac6:
	cmp #$15                 ; $9ac6: c9 15
	bne loc_9ad1             ; $9ac8: d0 07

; === Block $9aca-$9ace (Code) ===
	ldx #$0f                 ; $9aca: a2 0f
	lda #$02                 ; $9acc: a9 02
	jmp loc_d9e2             ; $9ace: 4c e2 d9

; === Block $9acb-$9ace (Code) ===
	*slo $02a9               ; $9acb: 0f a9 02

; === Block $9acd-$9ace (Code) ===
	*jam                     ; $9acd: 02

; === Block $9ad0-$9ad9 (Code) ===
	cmp $0fc9,y              ; $9ad0: d9 c9 0f
	bcs loc_9adc             ; $9ad3: b0 07
loc_9ad5:
	cmp #$06                 ; $9ad5: c9 06
	beq loc_9adc             ; $9ad7: f0 03
loc_9ad9:
	jmp loc_da55             ; $9ad9: 4c 55 da

; === Block $9ad1-$9ad3 (Code) ===
loc_9ad1:
	cmp #$0f                 ; $9ad1: c9 0f

; === Block $9adc-$9ae0 (Code) ===
loc_9adc:
	ldx #$12                 ; $9adc: a2 12
	lda #$02                 ; $9ade: a9 02
	jmp loc_d9e2             ; $9ae0: 4c e2 d9

; === Block $9ae3-$9ae5 (Code) ===
loc_9ae3:
	cmp #$08                 ; $9ae3: c9 08

; === Block $9aea-$9aea (Code) ===
	jmp loc_cfd9             ; $9aea: 4c d9 cf

; === Block $9aec-$9af9 (Code) ===
	*dcp $06c9               ; $9aec: cf c9 06
	bne loc_9b34             ; $9aef: d0 43
loc_9af1:
	lda $16                  ; $9af1: a5 16
	cmp #$20                 ; $9af3: c9 20
	beq loc_9afd             ; $9af5: f0 06
loc_9af7:
	lda $45                  ; $9af7: a5 45

; === Block $9aed-$9aef (Code) ===
	cmp #$06                 ; $9aed: c9 06

; === Block $9b06-$9b1c (Code) ===
	sta $3a                  ; $9b06: 85 3a
	sta $8e                  ; $9b08: 85 8e
	sta $90                  ; $9b0a: 85 90
	lda #$2b                 ; $9b0c: a9 2b
	sta $3b                  ; $9b0e: 85 3b
	sta $8f                  ; $9b10: 85 8f
	sta $92                  ; $9b12: 85 92
	lda #$00                 ; $9b14: a9 00
	sta $91                  ; $9b16: 85 91
	sta $93                  ; $9b18: 85 93
	ldx #$04                 ; $9b1a: a2 04

; === Block $9b0d-$9b12 (Code) ===
	*anc #$85                ; $9b0d: 2b 85
	*rla $8f85,y             ; $9b0f: 3b 85 8f

; === Block $9b19-$9b29 (Code) ===
	*sha ($a2),y             ; $9b19: 93 a2
	*nop $06                 ; $9b1b: 04 06
	bcc loc_9b45             ; $9b1d: 90 26
loc_9b1f:
	sta ($06),y              ; $9b1f: 91 06
	*jam                     ; $9b21: 92
	rol $93                  ; $9b22: 26 93
	dex                      ; $9b24: ca
	bne loc_9b1c             ; $9b25: d0 f5
loc_9b27:
	lda #$81                 ; $9b27: a9 81
	brk                      ; $9b29: 00

; === Block $9b34-$9b34 (Code) ===
loc_9b34:
	jmp loc_da55             ; $9b34: 4c 55 da

; === Block $9b3b-$9b47 (Code) ===
	*isc $7420,x             ; $9b3b: ff 20 74
	*isc $19a9,x             ; $9b3e: ff a9 19
	sta $2001                ; $9b41: 8d 01 20 PPUMASK
	jsr loc_ff74             ; $9b44: 20 74 ff

; === Block $9b3d-$9b41 (Code) ===
	*nop $ff,x               ; $9b3d: 74 ff
	lda #$19                 ; $9b3f: a9 19

; === Block $9b86-$9b92 (Code) ===
	*dcp $a6,x               ; $9b86: d7 a6
	*dcp $a5,x               ; $9b88: d7 a5
	dec $dd                  ; $9b8a: c6 dd
	*sre ($9d),y             ; $9b8c: 53 9d
	bcs loc_9b93             ; $9b8e: b0 03
loc_9b90:
	lda #$32                 ; $9b90: a9 32
	rts                      ; $9b92: 60

; === Block $9b89-$9b8e (Code) ===
	lda $c6                  ; $9b89: a5 c6
	cmp $9d53,x              ; $9b8b: dd 53 9d

; === Block $9b93-$9b98 (Code) ===
loc_9b93:
	sbc $9d53,x              ; $9b93: fd 53 9d
	sta $c6                  ; $9b96: 85 c6

; === Block $9ba9-$9bac (Code) ===
	jsr loc_db37             ; $9ba9: 20 37 db
loc_9bac:
	brk                      ; $9bac: 00

; === Block $9baf-$9bb5 (Code) ===
	lda $d7                  ; $9baf: a5 d7
	pha                      ; $9bb1: 48
	jsr loc_c6f0             ; $9bb2: 20 f0 c6
loc_9bb5:
	brk                      ; $9bb5: 00

; === Block $9bb8-$9bd5 (Code) ===
	jsr loc_c55b             ; $9bb8: 20 5b c5
loc_9bbb:
	lda $95                  ; $9bbb: a5 95
	and #$07                 ; $9bbd: 29 07
	clc                      ; $9bbf: 18
	adc #$0a                 ; $9bc0: 69 0a
	clc                      ; $9bc2: 18
	adc $c5                  ; $9bc3: 65 c5
	bcs loc_9bcb             ; $9bc5: b0 04
loc_9bc7:
	cmp $ca                  ; $9bc7: c5 ca
	bcc loc_9bcd             ; $9bc9: 90 02
loc_9bcb:
	lda $ca                  ; $9bcb: a5 ca
loc_9bcd:
	sta $c5                  ; $9bcd: 85 c5
	jsr loc_ee28             ; $9bcf: 20 28 ee
loc_9bd2:
	jsr loc_c6f0             ; $9bd2: 20 f0 c6
loc_9bd5:
	brk                      ; $9bd5: 00

; === Block $9bbe-$9bc0 (Code) ===
	*slo $18                 ; $9bbe: 07 18

; === Block $9bc1-$9bc2 (Code) ===
	asl                      ; $9bc1: 0a

; === Block $9bc8-$9bc9 (Code) ===
	dex                      ; $9bc8: ca

; === Block $9c29-$9c35 (Code) ===
	and loc_d94c,x           ; $9c29: 3d 4c d9
	*dcp $f020               ; $9c2c: cf 20 f0
	dec $07      