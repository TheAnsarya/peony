; Disassembled by Peony
; Platform: NES
; Mapper: MMC1
; Mapper: 1
; MapperName: MMC1
; PRG: 64K
; CHR: 16K
; Banks: 4


; ===========================================================================
; BANK 0
; ===========================================================================

	.bank 0
	.org $8000

; --- Block at $8000-$8000 ---
bank0_start:
	brk                     ; 8000: 00          


; ===========================================================================
; BANK 1
; ===========================================================================

	.bank 1
	.org $8000

; --- Block at $8000-$8013 ---
bank0_start:
	bit $af                 ; 8000: 24 af       
	cpy $ab                 ; 8002: c4 ab       
	sei                     ; 8004: 78          
	sta ($5e,x)             ; 8005: 81 5e       
	sta ($a0,x)             ; 8007: 81 a0       
	sta ($62,x)             ; 8009: 81 62       
	*sha ($00),y            ; 800b: 93 00       
	brk                     ; 800d: 00           Bank call: bank 0, Bank0_Func00
	.byte $00               ; 800e: 00           Function index
	.byte $00               ; 800f: 00           Target bank
	*sre $0099              ; 8010: 4f 99 00    
	brk                     ; 8013: 00          


; ===========================================================================
; BANK 2
; ===========================================================================

	.bank 2
	.org $8000

; --- Block at $8000-$8061 ---
bank0_start:
	bcs $7fbe               ; 8000: b0 bc       
loc_8002:
	plp                     ; 8002: 28          
	*nop #$86               ; 8003: 80 86       
	*nop #$19               ; 8005: 82 19       
	sta $13                 ; 8007: 85 13       
	*sax $4c                ; 8009: 87 4c       
	*nop #$12               ; 800b: 89 12       
	sta $906e               ; 800d: 8d 6e 90    
	*jam                    ; 8010: 42          
	sty $1e,x               ; 8011: 94 1e       
	tya                     ; 8013: 98          
	dey                     ; 8014: 88          
	*shy $9f3f,x            ; 8015: 9c 3f 9f    
	txa                     ; 8018: 8a          
	ldx #$dc                ; 8019: a2 dc       
	ldx $2e                 ; 801b: a6 2e       
	tax                     ; 801d: aa          
	adc ($ac,x)             ; 801e: 61 ac       
	plp                     ; 8020: 28          
	ldx $afee               ; 8021: ae ee af    
	*xaa #$b6               ; 8024: 8b b6       
	adc $ba                 ; 8026: 65 ba       
loc_8028:
	*nop $5f,x              ; 8028: f4 5f       
	ora ($0a),y             ; 802a: 11 0a       
	ora $5f11,x             ; 802c: 1d 11 5f    
	jsr $1418               ; 802f: 20 18 14    
loc_8032:
	asl $5f17               ; 8032: 0e 17 5f    
	asl $4719,x             ; 8035: 1e 19 47    
	*nop $fd60,x            ; 8038: fc 60 fd    
	*rla $11,x              ; 803b: 37 11       
	clc                     ; 803d: 18          
	asl $0a5f,x             ; 803e: 1e 5f 0a    
	*slo $5f1d,y            ; 8041: 1b 1d 5f    
	ora $0a0e               ; 8044: 0d 0e 0a    
	ora $fc47               ; 8047: 0d 47 fc    
	bvc loc_8083            ; 804a: 50 37       
loc_804c:
	ora ($18),y             ; 804c: 11 18       
	asl $0a5f,x             ; 804e: 1e 5f 0a    
	*slo $5f1d,y            ; 8051: 1b 1d 5f    
	*nop $1b1d,x            ; 8054: 1c 1d 1b    
	clc                     ; 8057: 18          
	*slo $10,x              ; 8058: 17 10       
	*sre $170e,x            ; 805a: 5f 0e 17    
	clc                     ; 805d: 18          
	asl $1110,x             ; 805e: 1e 10 11    
	jmp $3afd               ; 8061: 4c fd 3a     PPUSCROLL

; --- Block at $8083-$808a ---
loc_8083:
	clc                     ; 8083: 18          
	*slo $15,x              ; 8084: 17 15       
	clc                     ; 8086: 18          
	*slo $4b0d,y            ; 8087: 1b 0d 4b    
	rti                     ; 808a: 40          


; ===========================================================================
; BANK 3
; ===========================================================================

	.bank 3
	.org $8000

; --- Block at $8028-$8031 ---
loc_8028:
	asl                     ; 8028: 0a          
	ora $08                 ; 8029: 05 08       
	sta ($0a),y             ; 802b: 91 0a       
	sta $08                 ; 802d: 85 08       
	pla                     ; 802f: 68          
	tay                     ; 8030: a8          
	rts                     ; 8031: 60          

; --- Block at $c6bb-$c6c8 ---
loc_c6bb:
	jsr loc_ff74            ; c6bb: 20 74 ff    
loc_c6be:
	ldx #$00                ; c6be: a2 00       
	lda #$f0                ; c6c0: a9 f0       
	sta $0200,x             ; c6c2: 9d 00 02    
	inx                     ; c6c5: e8          
	bne $c6c2               ; c6c6: d0 fa       
loc_c6c8:
	rts                     ; c6c8: 60          

; --- Block at $c9b5-$c9cb ---
loc_c9b5:
	lda #$00                ; c9b5: a9 00       
	jsr loc_ff91            ; c9b7: 20 91 ff    
loc_c9ba:
	lda #$00                ; c9ba: a9 00       
	tax                     ; c9bc: aa          
	sta $600a               ; c9bd: 8d 0a 60    
	sta $602f               ; c9c0: 8d 2f 60    
	sta $601c,x             ; c9c3: 9d 1c 60    
	inx                     ; c9c6: e8          
	cpx #$10                ; c9c7: e0 10       
	bcc $c9c3               ; c9c9: 90 f8       
loc_c9cb:
	brk                     ; c9cb: 00          

; --- Block at $fc80-$fc87 ---
loc_fc80:
	pha                     ; fc80: 48          
	lda #$03                ; fc81: a9 03       
	jsr loc_ff96            ; fc83: 20 96 ff    
loc_fc86:
	pla                     ; fc86: 68          
	rts                     ; fc87: 60          

; --- Block at $fcbd-$fceb ---
loc_fcbd:
	sta $37                 ; fcbd: 85 37       
	stx $38                 ; fcbf: 86 38       
	lda $6004               ; fcc1: ad 04 60    
	pha                     ; fcc4: 48          
	php                     ; fcc5: 08          
	lda $6004               ; fcc6: ad 04 60    
	sta $6006               ; fcc9: 8d 06 60    
	jsr loc_fcec            ; fccc: 20 ec fc    
loc_fccf:
	lda #$4c                ; fccf: a9 4c       
	sta $30                 ; fcd1: 85 30       
	ldx $38                 ; fcd3: a6 38       
	lda $37                 ; fcd5: a5 37       
	plp                     ; fcd7: 28          
	jsr $0030               ; fcd8: 20 30 00    
loc_fcdb:
	php                     ; fcdb: 08          
	sta $37                 ; fcdc: 85 37       
	pla                     ; fcde: 68          
	sta $30                 ; fcdf: 85 30       
	pla                     ; fce1: 68          
	jsr loc_ff91            ; fce2: 20 91 ff    
loc_fce5:
	lda $30                 ; fce5: a5 30       
	pha                     ; fce7: 48          
	lda $37                 ; fce8: a5 37       
	plp                     ; fcea: 28          
	rts                     ; fceb: 60          

; --- Block at $fcec-$fcff ---
loc_fcec:
	lda $30                 ; fcec: a5 30       
	jsr loc_ff91            ; fcee: 20 91 ff    
loc_fcf1:
	lda $31                 ; fcf1: a5 31       
	asl                     ; fcf3: 0a          
	tax                     ; fcf4: aa          
	lda bank0_start,x       ; fcf5: bd 00 80    
	sta $31                 ; fcf8: 85 31       
	lda $8001,x             ; fcfa: bd 01 80    
	sta $32                 ; fcfd: 85 32       
	rts                     ; fcff: 60          

; --- Block at $fd00-$fd1b ---
loc_fd00:
	sta $37                 ; fd00: 85 37       
	stx $38                 ; fd02: 86 38       
	lda $6004               ; fd04: ad 04 60    
	pha                     ; fd07: 48          
	jsr loc_fcec            ; fd08: 20 ec fc    
loc_fd0b:
	pla                     ; fd0b: 68          
	jsr loc_ff91            ; fd0c: 20 91 ff    
loc_fd0f:
	ldx $38                 ; fd0f: a6 38       
	lda $31                 ; fd11: a5 31       
	sta $00,x               ; fd13: 95 00       
	lda $32                 ; fd15: a5 32       
	sta $01,x               ; fd17: 95 01       
	lda $37                 ; fd19: a5 37       
	rts                     ; fd1b: 60          

; --- Block at $fd3a-$fd74 ---
entry_fd3a:
	sei                     ; fd3a: 78          
	php                     ; fd3b: 08          
	bit $4015               ; fd3c: 2c 15 40     SND_CHN
	sta $37                 ; fd3f: 85 37       
	stx $38                 ; fd41: 86 38       
	sty $39                 ; fd43: 84 39       
	tsx                     ; fd45: ba          
	lda $0103,x             ; fd46: bd 03 01    
	sec                     ; fd49: 38          
	sbc #$01                ; fd4a: e9 01       
	sta $33                 ; fd4c: 85 33       
	lda $0104,x             ; fd4e: bd 04 01    
	sbc #$00                ; fd51: e9 00       
	sta $34                 ; fd53: 85 34       
	ldy #$01                ; fd55: a0 01       
	lda ($33),y             ; fd57: b1 33       
	pha                     ; fd59: 48          
	and #$08                ; fd5a: 29 08       
	cmp #$08                ; fd5c: c9 08       
	pla                     ; fd5e: 68          
	ror                     ; fd5f: 6a          
	lsr                     ; fd60: 4a          
	lsr                     ; fd61: 4a          
	lsr                     ; fd62: 4a          
	sta $30                 ; fd63: 85 30       
	dey                     ; fd65: 88          
	lda ($33),y             ; fd66: b1 33       
	bmi loc_fd77            ; fd68: 30 0d       
loc_fd6a:
	sta $31                 ; fd6a: 85 31       
	ldy $39                 ; fd6c: a4 39       
	ldx $38                 ; fd6e: a6 38       
	plp                     ; fd70: 28          
	pla                     ; fd71: 68          
	lda $37                 ; fd72: a5 37       
	jmp loc_fcbd            ; fd74: 4c bd fc    

; --- Block at $fd77-$fd83 ---
loc_fd77:
	and #$3f                ; fd77: 29 3f       
	sta $31                 ; fd79: 85 31       
	ldy $39                 ; fd7b: a4 39       
	ldx $38                 ; fd7d: a6 38       
	plp                     ; fd7f: 28          
	pla                     ; fd80: 68          
	lda $37                 ; fd81: a5 37       
	jmp loc_fd00            ; fd83: 4c 00 fd    

; --- Block at $fd86-$fdf1 ---
loc_fd86:
	cld                     ; fd86: d8          
	lda #$10                ; fd87: a9 10       
	sta $2000               ; fd89: 8d 00 20     PPUCTRL
	lda $2002               ; fd8c: ad 02 20     PPUSTATUS
	bmi $fd8c               ; fd8f: 30 fb       
loc_fd91:
	lda $2002               ; fd91: ad 02 20     PPUSTATUS
	bpl loc_fd91            ; fd94: 10 fb       
loc_fd96:
	lda $2002               ; fd96: ad 02 20     PPUSTATUS
	bmi loc_fd96            ; fd99: 30 fb       
loc_fd9b:
	lda #$00                ; fd9b: a9 00       
	sta $2001               ; fd9d: 8d 01 20     PPUMASK
	ldx #$ff                ; fda0: a2 ff       
	txs                     ; fda2: 9a          
	tax                     ; fda3: aa          
	sta $602c               ; fda4: 8d 2c 60    
	sta $00,x               ; fda7: 95 00       
	sta $0300,x             ; fda9: 9d 00 03    
	sta $0400,x             ; fdac: 9d 00 04    
	sta $0500,x             ; fdaf: 9d 00 05    
	sta $0600,x             ; fdb2: 9d 00 06    
	sta $0700,x             ; fdb5: 9d 00 07    
	inx                     ; fdb8: e8          
	bne $fda7               ; fdb9: d0 ec       
loc_fdbb:
	jsr loc_fc80            ; fdbb: 20 80 fc    
loc_fdbe:
	sta $6004               ; fdbe: 8d 04 60    
	lda #$1e                ; fdc1: a9 1e       
	sta $6001               ; fdc3: 8d 01 60    
	lda #$00                ; fdc6: a9 00       
	sta $6002               ; fdc8: 8d 02 60    
	sta $6003               ; fdcb: 8d 03 60    
	jsr loc_fdf4            ; fdce: 20 f4 fd    
loc_fdd1:
	lda $2002               ; fdd1: ad 02 20     PPUSTATUS
	lda #$10                ; fdd4: a9 10       
	sta $2006               ; fdd6: 8d 06 20     PPUADDR
	lda #$00                ; fdd9: a9 00       
	sta $2006               ; fddb: 8d 06 20     PPUADDR
	ldx #$10                ; fdde: a2 10       
	sta $2007               ; fde0: 8d 07 20     PPUDATA
	dex                     ; fde3: ca          
	bne $fde0               ; fde4: d0 fa       
loc_fde6:
	lda #$88                ; fde6: a9 88       
	sta $2000               ; fde8: 8d 00 20     PPUCTRL
	jsr loc_c6bb            ; fdeb: 20 bb c6    
loc_fdee:
	jsr loc_ff74            ; fdee: 20 74 ff    
loc_fdf1:
	jmp loc_c9b5            ; fdf1: 4c b5 c9    

; --- Block at $fdf4-$fe06 ---
loc_fdf4:
	inc $ffdf               ; fdf4: ee df ff    
	lda $6001               ; fdf7: ad 01 60    
	jsr loc_fe09            ; fdfa: 20 09 fe    
loc_fdfd:
	lda $6002               ; fdfd: ad 02 60    
	jsr loc_ffac            ; fe00: 20 ac ff    
loc_fe03:
	lda $6003               ; fe03: ad 03 60    
	jmp loc_ffc2            ; fe06: 4c c2 ff    

; --- Block at $fe09-$fe1f ---
loc_fe09:
	sta $6001               ; fe09: 8d 01 60    
	sta $9fff               ; fe0c: 8d ff 9f    
	lsr                     ; fe0f: 4a          
	sta $9fff               ; fe10: 8d ff 9f    
	lsr                     ; fe13: 4a          
	sta $9fff               ; fe14: 8d ff 9f    
	lsr                     ; fe17: 4a          
	sta $9fff               ; fe18: 8d ff 9f    
	lsr                     ; fe1b: 4a          
	sta $9fff               ; fe1c: 8d ff 9f    
	rts                     ; fe1f: 60          

; --- Block at $fe20-$fe55 ---
loc_fe20:
	ldy #$01                ; fe20: a0 01       
	lda $0300,x             ; fe22: bd 00 03    
	bpl loc_fe38            ; fe25: 10 11       
loc_fe27:
	tay                     ; fe27: a8          
	lsr                     ; fe28: 4a          
	lsr                     ; fe29: 4a          
	lsr                     ; fe2a: 4a          
	lsr                     ; fe2b: 4a          
	and #$04                ; fe2c: 29 04       
	ora #$88                ; fe2e: 09 88       
	sta $2000               ; fe30: 8d 00 20     PPUCTRL
	tya                     ; fe33: 98          
	inx                     ; fe34: e8          
	ldy $0300,x             ; fe35: bc 00 03    
loc_fe38:
	inx                     ; fe38: e8          
	and #$3f                ; fe39: 29 3f       
	sta $2006               ; fe3b: 8d 06 20     PPUADDR
	lda $0300,x             ; fe3e: bd 00 03    
	inx                     ; fe41: e8          
	sta $2006               ; fe42: 8d 06 20     PPUADDR
	lda $0300,x             ; fe45: bd 00 03    
	inx                     ; fe48: e8          
	sta $2007               ; fe49: 8d 07 20     PPUDATA
	dey                     ; fe4c: 88          
	bne $fe45               ; fe4d: d0 f6       
loc_fe4f:
	dec $03                 ; fe4f: c6 03       
	bne loc_fe20            ; fe51: d0 cd       
loc_fe53:
	beq loc_feb1            ; fe53: f0 5c       

; --- Block at $fe55-$fe5d ---
loc_fe55:
	jsr loc_ff2d            ; fe55: 20 2d ff    
loc_fe58:
	lda #$02                ; fe58: a9 02       
	sta $4014               ; fe5a: 8d 14 40     OAMDMA
	jmp $fee0               ; fe5d: 4c e0 fe    

; --- Block at $fe60-$fe67 ---
loc_fe60:
	lda #$02                ; fe60: a9 02       
	sta $4014               ; fe62: 8d 14 40     OAMDMA
	bne loc_feb1            ; fe65: d0 4a       

; --- Block at $fe67-$ff24 ---
entry_fe67:
	pha                     ; fe67: 48          
	txa                     ; fe68: 8a          
	pha                     ; fe69: 48          
	tya                     ; fe6a: 98          
	pha                     ; fe6b: 48          
	tsx                     ; fe6c: ba          
	lda $0106,x             ; fe6d: bd 06 01    
	cmp #$ff                ; fe70: c9 ff       
	bne loc_fe55            ; fe72: d0 e1       
loc_fe74:
	lda $0105,x             ; fe74: bd 05 01    
	cmp #$77                ; fe77: c9 77       
	bcc loc_fe55            ; fe79: 90 da       
loc_fe7b:
	cmp #$7d                ; fe7b: c9 7d       
	bcs loc_fe55            ; fe7d: b0 d6       
loc_fe7f:
	lda $2002               ; fe7f: ad 02 20     PPUSTATUS
	inc $4f                 ; fe82: e6 4f       
	lda $03                 ; fe84: a5 03       
	beq loc_fe60            ; fe86: f0 d8       
loc_fe88:
	cmp #$08                ; fe88: c9 08       
	bcs loc_fe91            ; fe8a: b0 05       
loc_fe8c:
	lda #$02                ; fe8c: a9 02       
	sta $4014               ; fe8e: 8d 14 40     OAMDMA
loc_fe91:
	ldx #$00                ; fe91: a2 00       
	lda $602c               ; fe93: ad 2c 60    
	bmi loc_fe20            ; fe96: 30 88       
loc_fe98:
	lda $0300,x             ; fe98: bd 00 03    
	sta $2006               ; fe9b: 8d 06 20     PPUADDR
	lda $0301,x             ; fe9e: bd 01 03    
	sta $2006               ; fea1: 8d 06 20     PPUADDR
	lda $0302,x             ; fea4: bd 02 03    
	sta $2007               ; fea7: 8d 07 20     PPUDATA
	inx                     ; feaa: e8          
	inx                     ; feab: e8          
	inx                     ; feac: e8          
	cpx $04                 ; fead: e4 04       
	bne loc_fe98            ; feaf: d0 e7       
loc_feb1:
	lda #$3f                ; feb1: a9 3f       
	sta $2006               ; feb3: 8d 06 20     PPUADDR
	lda #$00                ; feb6: a9 00       
	sta $02                 ; feb8: 85 02       
	sta $03                 ; feba: 85 03       
	sta $04                 ; febc: 85 04       
	sta $602c               ; febe: 8d 2c 60    
	sta $2006               ; fec1: 8d 06 20     PPUADDR
	lda #$0f                ; fec4: a9 0f       
	sta $2007               ; fec6: 8d 07 20     PPUDATA
	lda $06                 ; fec9: a5 06       
	bne loc_fed1            ; fecb: d0 04       
loc_fecd:
	lda #$88                ; fecd: a9 88       
	bne loc_fed3            ; fecf: d0 02       
loc_fed1:
	lda #$89                ; fed1: a9 89       
loc_fed3:
	sta $2000               ; fed3: 8d 00 20     PPUCTRL
	lda $05                 ; fed6: a5 05       
	sta $2005               ; fed8: 8d 05 20     PPUSCROLL
	lda $07                 ; fedb: a5 07       
	sta $2005               ; fedd: 8d 05 20     PPUSCROLL
	jsr loc_fdf4            ; fee0: 20 f4 fd    
loc_fee3:
	lda $6005               ; fee3: ad 05 60    
	bne loc_fef0            ; fee6: d0 08       
loc_fee8:
	lda #$01                ; fee8: a9 01       
	jsr loc_ff96            ; feea: 20 96 ff    
loc_feed:
	jsr loc_8028            ; feed: 20 28 80    
loc_fef0:
	lda $6004               ; fef0: ad 04 60    
	jsr loc_ff91            ; fef3: 20 91 ff    
loc_fef6:
	tsx                     ; fef6: ba          
	lda $0106,x             ; fef7: bd 06 01    
	sta $36                 ; fefa: 85 36       
	cmp #$ff                ; fefc: c9 ff       
	bne loc_ff10            ; fefe: d0 10       
loc_ff00:
	lda $0105,x             ; ff00: bd 05 01    
	cmp #$96                ; ff03: c9 96       
	bcc loc_ff10            ; ff05: 90 09       
loc_ff07:
	cmp #$d6                ; ff07: c9 d6       
	bcs loc_ff10            ; ff09: b0 05       
loc_ff0b:
	lda #$d6                ; ff0b: a9 d6       
	sta $0105,x             ; ff0d: 9d 05 01    
loc_ff10:
	lda $0105,x             ; ff10: bd 05 01    
	sta $35                 ; ff13: 85 35       
	ldy #$00                ; ff15: a0 00       
	lda ($35),y             ; ff17: b1 35       
	and #$0f                ; ff19: 29 0f       
	cmp #$07                ; ff1b: c9 07       
	beq loc_ff25            ; ff1d: f0 06       
loc_ff1f:
	pla                     ; ff1f: 68          
	tay                     ; ff20: a8          
	pla                     ; ff21: 68          
	tax                     ; ff22: aa          
	pla                     ; ff23: 68          
	rti                     ; ff24: 40          

; --- Block at $ff25-$ff2a ---
loc_ff25:
	pla                     ; ff25: 68          
	tay                     ; ff26: a8          
	pla                     ; ff27: 68          
	tax                     ; ff28: aa          
	pla                     ; ff29: 68          
	jmp entry_fd3a          ; ff2a: 4c 3a fd    

; --- Block at $ff2d-$ff53 ---
loc_ff2d:
	lda #$3f                ; ff2d: a9 3f       
	sta $2006               ; ff2f: 8d 06 20     PPUADDR
	lda #$00                ; ff32: a9 00       
	sta $2006               ; ff34: 8d 06 20     PPUADDR
	lda #$0f                ; ff37: a9 0f       
	sta $2007               ; ff39: 8d 07 20     PPUDATA
	lda $06                 ; ff3c: a5 06       
	bne loc_ff44            ; ff3e: d0 04       
loc_ff40:
	lda #$88                ; ff40: a9 88       
	bne loc_ff46            ; ff42: d0 02       
loc_ff44:
	lda #$89                ; ff44: a9 89       
loc_ff46:
	sta $2000               ; ff46: 8d 00 20     PPUCTRL
	lda $05                 ; ff49: a5 05       
	sta $2005               ; ff4b: 8d 05 20     PPUSCROLL
	lda $07                 ; ff4e: a5 07       
	sta $2005               ; ff50: 8d 05 20     PPUSCROLL
	rts                     ; ff53: 60          

; --- Block at $ff74-$ff7c ---
loc_ff74:
	lda #$01                ; ff74: a9 01       
	sta $02                 ; ff76: 85 02       
	lda $02                 ; ff78: a5 02       
	bne $ff78               ; ff7a: d0 fc       
loc_ff7c:
	rts                     ; ff7c: 60          

; --- Block at $ff91-$ff96 ---
loc_ff91:
	sta $6004               ; ff91: 8d 04 60    
	nop                     ; ff94: ea          
	nop                     ; ff95: ea          

; --- Block at $ff96-$ffab ---
loc_ff96:
	sta $ffff               ; ff96: 8d ff ff    
	lsr                     ; ff99: 4a          
	sta $ffff               ; ff9a: 8d ff ff    
	lsr                     ; ff9d: 4a          
	sta $ffff               ; ff9e: 8d ff ff    
	lsr                     ; ffa1: 4a          
	sta $ffff               ; ffa2: 8d ff ff    
	lsr                     ; ffa5: 4a          
	sta $ffff               ; ffa6: 8d ff ff    
	nop                     ; ffa9: ea          
	nop                     ; ffaa: ea          
	rts                     ; ffab: 60          

; --- Block at $ffac-$ffc1 ---
loc_ffac:
	sta $bfff               ; ffac: 8d ff bf    
	lsr                     ; ffaf: 4a          
	sta $bfff               ; ffb0: 8d ff bf    
	lsr                     ; ffb3: 4a          
	sta $bfff               ; ffb4: 8d ff bf    
	lsr                     ; ffb7: 4a          
	sta $bfff               ; ffb8: 8d ff bf    
	lsr                     ; ffbb: 4a          
	sta $bfff               ; ffbc: 8d ff bf    
	nop                     ; ffbf: ea          
	nop                     ; ffc0: ea          
	rts                     ; ffc1: 60          

; --- Block at $ffc2-$ffd7 ---
loc_ffc2:
	sta $dfff               ; ffc2: 8d ff df    
	lsr                     ; ffc5: 4a          
	sta $dfff               ; ffc6: 8d ff df    
	lsr                     ; ffc9: 4a          
	sta $dfff               ; ffca: 8d ff df    
	lsr                     ; ffcd: 4a          
	sta $dfff               ; ffce: 8d ff df    
	lsr                     ; ffd1: 4a          
	sta $dfff               ; ffd2: 8d ff df    
	nop                     ; ffd5: ea          
	nop                     ; ffd6: ea          
	rts                     ; ffd7: 60          

; --- Block at $ffd8-$ffdc ---
reset:
	sei                     ; ffd8: 78          
	inc $ffdf               ; ffd9: ee df ff    
	jmp loc_fd86            ; ffdc: 4c 86 fd    

