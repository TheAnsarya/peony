; 🌺 Peony Disassembly
; ROM: Dragon Warrior (U) (PRG0) [!].nes
; Platform: NES
; Size: 81936 bytes
; Mapper: MMC1
; Labels: 315

; === Block $c6bb-$c6c8 (Code) ===
clear_sprites_init:
	jsr disable_rendering    ; $c6bb: 20 74 ff
clear_sprites_loop:
	ldx #$00                 ; $c6be: a2 00
	lda #$f0                 ; $c6c0: a9 f0
	sta sprite_ram,x         ; $c6c2: 9d 00 02
	inx                      ; $c6c5: e8
	bne $c6c2                ; $c6c6: d0 fa
loc_c6c8:
	rts                      ; $c6c8: 60

; === Block $c9b5-$c9cb (Code) ===
init_game_state:
	lda #$00                 ; $c9b5: a9 00
	jsr set_bank             ; $c9b7: 20 91 ff
clear_state_loop:
	lda #$00                 ; $c9ba: a9 00
	tax                      ; $c9bc: aa
	sta dragonlord_palette   ; $c9bd: 8d 0a 60
	sta char_direction       ; $c9c0: 8d 2f 60
	sta treasure_x_pos,x     ; $c9c3: 9d 1c 60
	inx                      ; $c9c6: e8
	cpx #$10                 ; $c9c7: e0 10
	bcc $c9c3                ; $c9c9: 90 f8
loc_c9cb:
	brk                      ; $c9cb: 00

; === Block $fc80-$fc87 (Code) ===
switch_to_bank3:
	pha                      ; $fc80: 48
	lda #$03                 ; $fc81: a9 03
	jsr set_bank_direct      ; $fc83: 20 96 ff
loc_fc86:
	pla                      ; $fc86: 68
	rts                      ; $fc87: 60

; === Block $fcbd-$fceb (Code) ===
bank_call_wrapper:
	sta irq_store_a          ; $fcbd: 85 37
	stx irq_store_x          ; $fcbf: 86 38
	lda active_bank          ; $fcc1: ad 04 60
	pha                      ; $fcc4: 48
	php                      ; $fcc5: 08
	lda active_bank          ; $fcc6: ad 04 60
	sta $6006                ; $fcc9: 8d 06 60
	jsr setup_bank_call      ; $fccc: 20 ec fc
bank_call_execute:
	lda #$4c                 ; $fccf: a9 4c
	sta jmp_func_ptr         ; $fcd1: 85 30
	ldx irq_store_x          ; $fcd3: a6 38
	lda irq_store_a          ; $fcd5: a5 37
	plp                      ; $fcd7: 28
	jsr jmp_func_ptr         ; $fcd8: 20 30 00
bank_call_return:
	php                      ; $fcdb: 08
	sta irq_store_a          ; $fcdc: 85 37
	pla                      ; $fcde: 68
	sta jmp_func_ptr         ; $fcdf: 85 30
	pla                      ; $fce1: 68
	jsr set_bank             ; $fce2: 20 91 ff
bank_call_cleanup:
	lda jmp_func_ptr         ; $fce5: a5 30
	pha                      ; $fce7: 48
	lda irq_store_a          ; $fce8: a5 37
	plp                      ; $fcea: 28
	rts                      ; $fceb: 60

; === Block $fcec-$fcff (Code) ===
setup_bank_call:
	lda jmp_func_ptr         ; $fcec: a5 30
	jsr set_bank             ; $fcee: 20 91 ff
lookup_function_ptr:
	lda bank_pointer_lo      ; $fcf1: a5 31
	asl                      ; $fcf3: 0a
	tax                      ; $fcf4: aa
	lda bank_pointers,x      ; $fcf5: bd 00 80
	sta bank_pointer_lo      ; $fcf8: 85 31
	lda $8001,x              ; $fcfa: bd 01 80
	sta bank_pointer_hi      ; $fcfd: 85 32
	rts                      ; $fcff: 60

; === Block $fd00-$fd1b (Code) ===
bank_switch_handler:
	sta irq_store_a          ; $fd00: 85 37
	stx irq_store_x          ; $fd02: 86 38
	lda active_bank          ; $fd04: ad 04 60
	pha                      ; $fd07: 48
	jsr setup_bank_call      ; $fd08: 20 ec fc
loc_fd0b:
	pla                      ; $fd0b: 68
	jsr set_bank             ; $fd0c: 20 91 ff
loc_fd0f:
	ldx irq_store_x          ; $fd0f: a6 38
	lda bank_pointer_lo      ; $fd11: a5 31
	sta gen_byte_00,x        ; $fd13: 95 00
	lda bank_pointer_hi      ; $fd15: a5 32
	sta gen_byte_01,x        ; $fd17: 95 01
	lda irq_store_a          ; $fd19: a5 37
	rts                      ; $fd1b: 60

; === Block $fd3a-$fd74 (Code) ===
irq_handler:
	sei                      ; $fd3a: 78 BRK-based bank call dispatcher - reads func index and bank from stack
	php                      ; $fd3b: 08
	bit $4015                ; $fd3c: 2c 15 40 SND_CHN
	sta irq_store_a          ; $fd3f: 85 37
	stx irq_store_x          ; $fd41: 86 38
	sty irq_store_y          ; $fd43: 84 39
	tsx                      ; $fd45: ba
	lda bank_func_dat_lo,x   ; $fd46: bd 03 01
	sec                      ; $fd49: 38
	sbc #$01                 ; $fd4a: e9 01
	sta bank_func_data_ptr_lo ; $fd4c: 85 33
	lda bank_func_dat_hi,x   ; $fd4e: bd 04 01
	sbc #$00                 ; $fd51: e9 00
	sta bank_func_data_ptr_hi ; $fd53: 85 34
	ldy #$01                 ; $fd55: a0 01
	lda (bank_func_data_ptr_lo),y ; $fd57: b1 33
	pha                      ; $fd59: 48
	and #$08                 ; $fd5a: 29 08
	cmp #$08                 ; $fd5c: c9 08
	pla                      ; $fd5e: 68
	ror                      ; $fd5f: 6a
	lsr                      ; $fd60: 4a
	lsr                      ; $fd61: 4a
	lsr                      ; $fd62: 4a
	sta jmp_func_ptr         ; $fd63: 85 30
	dey                      ; $fd65: 88
	lda (bank_func_data_ptr_lo),y ; $fd66: b1 33
	bmi loc_fd77             ; $fd68: 30 0d
loc_fd6a:
	sta bank_pointer_lo      ; $fd6a: 85 31
	ldy irq_store_y          ; $fd6c: a4 39
	ldx irq_store_x          ; $fd6e: a6 38
	plp                      ; $fd70: 28
	pla                      ; $fd71: 68
	lda irq_store_a          ; $fd72: a5 37
	jmp bank_call_wrapper    ; $fd74: 4c bd fc

; === Block $fd77-$fd83 (Code) ===
loc_fd77:
	and #$3f                 ; $fd77: 29 3f
	sta bank_pointer_lo      ; $fd79: 85 31
	ldy irq_store_y          ; $fd7b: a4 39
	ldx irq_store_x          ; $fd7d: a6 38
	plp                      ; $fd7f: 28
	pla                      ; $fd80: 68
	lda irq_store_a          ; $fd81: a5 37
	jmp bank_switch_handler  ; $fd83: 4c 00 fd

; === Block $fd86-$fdf1 (Code) ===
reset_continue:
	cld                      ; $fd86: d8
	lda #$10                 ; $fd87: a9 10
	sta $2000                ; $fd89: 8d 00 20 PPUCTRL
	lda $2002                ; $fd8c: ad 02 20 PPUSTATUS
	bmi $fd8c                ; $fd8f: 30 fb
loc_fd91:
	lda $2002                ; $fd91: ad 02 20 PPUSTATUS
	bpl loc_fd91             ; $fd94: 10 fb
loc_fd96:
	lda $2002                ; $fd96: ad 02 20 PPUSTATUS
	bmi loc_fd96             ; $fd99: 30 fb
loc_fd9b:
	lda #$00                 ; $fd9b: a9 00
	sta $2001                ; $fd9d: 8d 01 20 PPUMASK
	ldx #$ff                 ; $fda0: a2 ff
	txs                      ; $fda2: 9a
	tax                      ; $fda3: aa
	sta update_bg_tiles      ; $fda4: 8d 2c 60
	sta gen_byte_00,x        ; $fda7: 95 00
	sta block_ram,x          ; $fda9: 9d 00 03
	sta window_buffer_ram,x  ; $fdac: 9d 00 04
	sta $0500,x              ; $fdaf: 9d 00 05
	sta $0600,x              ; $fdb2: 9d 00 06
	sta $0700,x              ; $fdb5: 9d 00 07
	inx                      ; $fdb8: e8
	bne $fda7                ; $fdb9: d0 ec
loc_fdbb:
	jsr switch_to_bank3      ; $fdbb: 20 80 fc
loc_fdbe:
	sta active_bank          ; $fdbe: 8d 04 60
	lda #$1e                 ; $fdc1: a9 1e
	sta mmc1_config          ; $fdc3: 8d 01 60
	lda #$00                 ; $fdc6: a9 00
	sta active_nt0           ; $fdc8: 8d 02 60
	sta active_nt1           ; $fdcb: 8d 03 60
	jsr loc_fdf4             ; $fdce: 20 f4 fd
loc_fdd1:
	lda $2002                ; $fdd1: ad 02 20 PPUSTATUS
	lda #$10                 ; $fdd4: a9 10
	sta $2006                ; $fdd6: 8d 06 20 PPUADDR
	lda #$00                 ; $fdd9: a9 00
	sta $2006                ; $fddb: 8d 06 20 PPUADDR
	ldx #$10                 ; $fdde: a2 10
	sta $2007                ; $fde0: 8d 07 20 PPUDATA
	dex                      ; $fde3: ca
	bne $fde0                ; $fde4: d0 fa
loc_fde6:
	lda #$88                 ; $fde6: a9 88
	sta $2000                ; $fde8: 8d 00 20 PPUCTRL
	jsr clear_sprites_init   ; $fdeb: 20 bb c6
loc_fdee:
	jsr disable_rendering    ; $fdee: 20 74 ff
loc_fdf1:
	jmp init_game_state      ; $fdf1: 4c b5 c9

; === Block $fdf4-$fe06 (Code) ===
loc_fdf4:
	inc mapper_init_reg      ; $fdf4: ee df ff
	lda mmc1_config          ; $fdf7: ad 01 60
	jsr loc_fe09             ; $fdfa: 20 09 fe
loc_fdfd:
	lda active_nt0           ; $fdfd: ad 02 60
	jsr loc_ffac             ; $fe00: 20 ac ff
loc_fe03:
	lda active_nt1           ; $fe03: ad 03 60
	jmp loc_ffc2             ; $fe06: 4c c2 ff

; === Block $fe09-$fe1f (Code) ===
loc_fe09:
	sta mmc1_config          ; $fe09: 8d 01 60
	sta $9fff                ; $fe0c: 8d ff 9f
	lsr                      ; $fe0f: 4a
	sta $9fff                ; $fe10: 8d ff 9f
	lsr                      ; $fe13: 4a
	sta $9fff                ; $fe14: 8d ff 9f
	lsr                      ; $fe17: 4a
	sta $9fff                ; $fe18: 8d ff 9f
	lsr                      ; $fe1b: 4a
	sta $9fff                ; $fe1c: 8d ff 9f
	rts                      ; $fe1f: 60

; === Block $fe20-$fe55 (Code) ===
loc_fe20:
	ldy #$01                 ; $fe20: a0 01
	lda block_ram,x          ; $fe22: bd 00 03
	bpl loc_fe38             ; $fe25: 10 11
loc_fe27:
	tay                      ; $fe27: a8
	lsr                      ; $fe28: 4a
	lsr                      ; $fe29: 4a
	lsr                      ; $fe2a: 4a
	lsr                      ; $fe2b: 4a
	and #$04                 ; $fe2c: 29 04
	ora #$88                 ; $fe2e: 09 88
	sta $2000                ; $fe30: 8d 00 20 PPUCTRL
	tya                      ; $fe33: 98
	inx                      ; $fe34: e8
	ldy block_ram,x          ; $fe35: bc 00 03
loc_fe38:
	inx                      ; $fe38: e8
	and #$3f                 ; $fe39: 29 3f
	sta $2006                ; $fe3b: 8d 06 20 PPUADDR
	lda block_ram,x          ; $fe3e: bd 00 03
	inx                      ; $fe41: e8
	sta $2006                ; $fe42: 8d 06 20 PPUADDR
	lda block_ram,x          ; $fe45: bd 00 03
	inx                      ; $fe48: e8
	sta $2007                ; $fe49: 8d 07 20 PPUDATA
	dey                      ; $fe4c: 88
	bne $fe45                ; $fe4d: d0 f6
loc_fe4f:
	dec ppu_entry_count      ; $fe4f: c6 03
	bne loc_fe20             ; $fe51: d0 cd
loc_fe53:
	beq loc_feb1             ; $fe53: f0 5c

; === Block $fe55-$fe5d (Code) ===
nmi_continue:
	jsr loc_ff2d             ; $fe55: 20 2d ff
loc_fe58:
	lda #$02                 ; $fe58: a9 02
	sta $4014                ; $fe5a: 8d 14 40 OAMDMA
	jmp $fee0                ; $fe5d: 4c e0 fe

; === Block $fe60-$fe67 (Code) ===
loc_fe60:
	lda #$02                 ; $fe60: a9 02
	sta $4014                ; $fe62: 8d 14 40 OAMDMA
	bne loc_feb1             ; $fe65: d0 4a

; === Block $fe67-$ff24 (Code) ===
nmi_handler:
	pha                      ; $fe67: 48 Non-maskable interrupt handler (VBlank)
	txa                      ; $fe68: 8a
	pha                      ; $fe69: 48
	tya                      ; $fe6a: 98
	pha                      ; $fe6b: 48
	tsx                      ; $fe6c: ba
	lda $0106,x              ; $fe6d: bd 06 01
	cmp #$ff                 ; $fe70: c9 ff
	bne nmi_continue         ; $fe72: d0 e1
nmi_check_stack:
	lda $0105,x              ; $fe74: bd 05 01
	cmp #$77                 ; $fe77: c9 77
	bcc nmi_continue         ; $fe79: 90 da
nmi_range_check:
	cmp #$7d                 ; $fe7b: c9 7d
	bcs nmi_continue         ; $fe7d: b0 d6
nmi_vblank_wait:
	lda $2002                ; $fe7f: ad 02 20 PPUSTATUS
	inc frame_counter        ; $fe82: e6 4f
	lda ppu_entry_count      ; $fe84: a5 03
	beq loc_fe60             ; $fe86: f0 d8
loc_fe88:
	cmp #$08                 ; $fe88: c9 08
	bcs loc_fe91             ; $fe8a: b0 05
loc_fe8c:
	lda #$02                 ; $fe8c: a9 02
	sta $4014                ; $fe8e: 8d 14 40 OAMDMA
loc_fe91:
	ldx #$00                 ; $fe91: a2 00
	lda update_bg_tiles      ; $fe93: ad 2c 60
	bmi loc_fe20             ; $fe96: 30 88
loc_fe98:
	lda block_ram,x          ; $fe98: bd 00 03
	sta $2006                ; $fe9b: 8d 06 20 PPUADDR
	lda $0301,x              ; $fe9e: bd 01 03
	sta $2006                ; $fea1: 8d 06 20 PPUADDR
	lda $0302,x              ; $fea4: bd 02 03
	sta $2007                ; $fea7: 8d 07 20 PPUDATA
	inx                      ; $feaa: e8
	inx                      ; $feab: e8
	inx                      ; $feac: e8
	cpx ppu_buf_count        ; $fead: e4 04
	bne loc_fe98             ; $feaf: d0 e7
loc_feb1:
	lda #$3f                 ; $feb1: a9 3f
	sta $2006                ; $feb3: 8d 06 20 PPUADDR
	lda #$00                 ; $feb6: a9 00
	sta nmi_status           ; $feb8: 85 02
	sta ppu_entry_count      ; $feba: 85 03
	sta ppu_buf_count        ; $febc: 85 04
	sta update_bg_tiles      ; $febe: 8d 2c 60
	sta $2006                ; $fec1: 8d 06 20 PPUADDR
	lda #$0f                 ; $fec4: a9 0f
	sta $2007                ; $fec6: 8d 07 20 PPUDATA
	lda active_nametable     ; $fec9: a5 06
	bne loc_fed1             ; $fecb: d0 04
loc_fecd:
	lda #$88                 ; $fecd: a9 88
	bne loc_fed3             ; $fecf: d0 02
loc_fed1:
	lda #$89                 ; $fed1: a9 89
loc_fed3:
	sta $2000                ; $fed3: 8d 00 20 PPUCTRL
	lda scroll_x             ; $fed6: a5 05
	sta $2005                ; $fed8: 8d 05 20 PPUSCROLL
	lda scroll_y             ; $fedb: a5 07
	sta $2005                ; $fedd: 8d 05 20 PPUSCROLL
	jsr loc_fdf4             ; $fee0: 20 f4 fd
loc_fee3:
	lda sound_engine_status  ; $fee3: ad 05 60
	bne loc_fef0             ; $fee6: d0 08
loc_fee8:
	lda #$01                 ; $fee8: a9 01
	jsr set_bank_direct      ; $feea: 20 96 ff
loc_feed:
	jsr update_sound         ; $feed: 20 28 80
loc_fef0:
	lda active_bank          ; $fef0: ad 04 60
	jsr set_bank             ; $fef3: 20 91 ff
loc_fef6:
	tsx                      ; $fef6: ba
	lda $0106,x              ; $fef7: bd 06 01
	sta nmi_ptr_hi           ; $fefa: 85 36
	cmp #$ff                 ; $fefc: c9 ff
	bne loc_ff10             ; $fefe: d0 10
loc_ff00:
	lda $0105,x              ; $ff00: bd 05 01
	cmp #$96                 ; $ff03: c9 96
	bcc loc_ff10             ; $ff05: 90 09
loc_ff07:
	cmp #$d6                 ; $ff07: c9 d6
	bcs loc_ff10             ; $ff09: b0 05
loc_ff0b:
	lda #$d6                 ; $ff0b: a9 d6
	sta $0105,x              ; $ff0d: 9d 05 01
loc_ff10:
	lda $0105,x              ; $ff10: bd 05 01
	sta nmi_ptr_lo           ; $ff13: 85 35
	ldy #$00                 ; $ff15: a0 00
	lda (nmi_ptr_lo),y       ; $ff17: b1 35
	and #$0f                 ; $ff19: 29 0f
	cmp #$07                 ; $ff1b: c9 07
	beq loc_ff25             ; $ff1d: f0 06
loc_ff1f:
	pla                      ; $ff1f: 68
	tay                      ; $ff20: a8
	pla                      ; $ff21: 68
	tax                      ; $ff22: aa
	pla                      ; $ff23: 68
	rti                      ; $ff24: 40

; === Block $ff25-$ff2a (Code) ===
loc_ff25:
	pla                      ; $ff25: 68
	tay                      ; $ff26: a8
	pla                      ; $ff27: 68
	tax                      ; $ff28: aa
	pla                      ; $ff29: 68
	jmp irq_handler          ; $ff2a: 4c 3a fd

; === Block $ff2d-$ff53 (Code) ===
loc_ff2d:
	lda #$3f                 ; $ff2d: a9 3f
	sta $2006                ; $ff2f: 8d 06 20 PPUADDR
	lda #$00                 ; $ff32: a9 00
	sta $2006                ; $ff34: 8d 06 20 PPUADDR
	lda #$0f                 ; $ff37: a9 0f
	sta $2007                ; $ff39: 8d 07 20 PPUDATA
	lda active_nametable     ; $ff3c: a5 06
	bne loc_ff44             ; $ff3e: d0 04
loc_ff40:
	lda #$88                 ; $ff40: a9 88
	bne loc_ff46             ; $ff42: d0 02
loc_ff44:
	lda #$89                 ; $ff44: a9 89
loc_ff46:
	sta $2000                ; $ff46: 8d 00 20 PPUCTRL
	lda scroll_x             ; $ff49: a5 05
	sta $2005                ; $ff4b: 8d 05 20 PPUSCROLL
	lda scroll_y             ; $ff4e: a5 07
	sta $2005                ; $ff50: 8d 05 20 PPUSCROLL
	rts                      ; $ff53: 60

; === Block $ff74-$ff7c (Code) ===
disable_rendering:
	lda #$01                 ; $ff74: a9 01
	sta nmi_status           ; $ff76: 85 02
	lda nmi_status           ; $ff78: a5 02
	bne $ff78                ; $ff7a: d0 fc
loc_ff7c:
	rts                      ; $ff7c: 60

; === Block $ff91-$ff96 (Code) ===
set_bank:
	sta active_bank          ; $ff91: 8d 04 60 Bank switching routine - writes to MMC1
	nop                      ; $ff94: ea
	nop                      ; $ff95: ea

; === Block $ff96-$ffab (Code) ===
set_bank_direct:
	sta $ffff                ; $ff96: 8d ff ff
	lsr                      ; $ff99: 4a
	sta $ffff                ; $ff9a: 8d ff ff
	lsr                      ; $ff9d: 4a
	sta $ffff                ; $ff9e: 8d ff ff
	lsr                      ; $ffa1: 4a
	sta $ffff                ; $ffa2: 8d ff ff
	lsr                      ; $ffa5: 4a
	sta $ffff                ; $ffa6: 8d ff ff
	nop                      ; $ffa9: ea
	nop                      ; $ffaa: ea
	rts                      ; $ffab: 60

; === Block $ffac-$ffc1 (Code) ===
loc_ffac:
	sta $bfff                ; $ffac: 8d ff bf
	lsr                      ; $ffaf: 4a
	sta $bfff                ; $ffb0: 8d ff bf
	lsr                      ; $ffb3: 4a
	sta $bfff                ; $ffb4: 8d ff bf
	lsr                      ; $ffb7: 4a
	sta $bfff                ; $ffb8: 8d ff bf
	lsr                      ; $ffbb: 4a
	sta $bfff                ; $ffbc: 8d ff bf
	nop                      ; $ffbf: ea
	nop                      ; $ffc0: ea
	rts                      ; $ffc1: 60

; === Block $ffc2-$ffd7 (Code) ===
loc_ffc2:
	sta $dfff                ; $ffc2: 8d ff df
	lsr                      ; $ffc5: 4a
	sta $dfff                ; $ffc6: 8d ff df
	lsr                      ; $ffc9: 4a
	sta $dfff                ; $ffca: 8d ff df
	lsr                      ; $ffcd: 4a
	sta $dfff                ; $ffce: 8d ff df
	lsr                      ; $ffd1: 4a
	sta $dfff                ; $ffd2: 8d ff df
	nop                      ; $ffd5: ea
	nop                      ; $ffd6: ea
	rts                      ; $ffd7: 60

; === Block $ffd8-$ffdc (Code) ===
reset:
	sei                      ; $ffd8: 78 Power-on/reset entry point
	inc mapper_init_reg      ; $ffd9: ee df ff
	jmp reset_continue       ; $ffdc: 4c 86 fd

