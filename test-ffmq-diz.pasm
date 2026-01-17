; 🌺 Peony Disassembly
; ROM: Final Fantasy - Mystic Quest (U) (V1.1).sfc
; Platform: SNES
; Size: 524288 bytes
; Mapper: FF MYSTIC QUEST
; Labels: 24

; === Block $8000-$80ef (Code) ===
reset:
	cpx #$34                 ; $8000: e0 34
	jsl loc_dc7a30           ; $8002: 22 30 7a dc
loc_8006:
	ora $64,x                ; $8006: 15 64
	asl $00de,x              ; $8008: 1e de 00
	ora ($32)                ; $800b: 12 32
	ply                      ; $800d: 7a
	bit $21,x                ; $800e: 34 21
	bpl loc_8001             ; $8010: 10 ef
loc_8012:
	beq loc_8015             ; $8012: f0 01
loc_8014:
	bpl loc_8005             ; $8014: 10 ef
loc_8016:
	ply                      ; $8016: 7a
	sbc $0f1200,x            ; $8017: ff 00 12 0f
	ora $01dddd              ; $801b: 0f dd dd 01
	ply                      ; $801f: 7a
	inc $ec00,x              ; $8020: fe 00 ec
	dec $dcff                ; $8023: ce ff dc
	bcs loc_8047             ; $8026: b0 1f
loc_8028:
	ror                      ; $8028: 6a
	ora $1d,x                ; $8029: 15 1d
	cpx $f3bd                ; $802b: ec bd f3
	and $beaa,x              ; $802e: 3d aa be
	ply                      ; $8031: 7a
	ora ($2f)                ; $8032: 12 2f
	sbc ($30,x)              ; $8034: e1 30
	cop #$45                 ; $8036: 02 45
	eor ($ed),y              ; $8038: 51 ed
	ply                      ; $803a: 7a
	ora $2d,s                ; $803b: 03 2d
	bne loc_8061             ; $803d: d0 22
loc_803f:
	bit $43                  ; $803f: 24 43
	ora ($01),y              ; $8041: 11 01
	ror                      ; $8043: 6a
	and $54                  ; $8044: 25 54
	and ($ee)                ; $8046: 32 ee
	cop #$35                 ; $8048: 02 35
	ror $3f,x                ; $804a: 76 3f
	ror                      ; $804c: 6a
	stp                      ; $804d: db
	lda $1100                ; $804e: ad 00 11
	jsl loc_ea4211           ; $8051: 22 11 42 ea TIMEUP
loc_8055:
	ply                      ; $8055: 7a
	dec $0ef0,x              ; $8056: de f0 0e
	bne loc_808d             ; $8059: d0 32
loc_805b:
	asl $cfcb                ; $805b: 0e cb cf
	ply                      ; $805e: 7a
	ora ($23),y              ; $805f: 11 23
loc_8061:
	ora $2fe0,x              ; $8061: 1d e0 2f
	sbc $7ad0fc,x            ; $8064: ff fc d0 7a
	asl $33e1                ; $8068: 0e e1 33
	jsr ($00d0,x)            ; $806b: fc d0 00
	cpx #$23                 ; $806e: e0 23
	ply                      ; $8070: 7a
	and ($db),y              ; $8071: 31 db
	cpx #$f0                 ; $8073: e0 f0
	ora ($ff,x)              ; $8075: 01 ff
	sbc ($13),y              ; $8077: f1 13
	ply                      ; $8079: 7a
	eor $fc,s                ; $807a: 43 fc
	ldx $2400,y              ; $807c: be 00 24
	eor ($1e,s),y            ; $807f: 53 1e
	inc $f26a                ; $8081: ee 6a f2
	rol $34df,x              ; $8084: 3e df 34
	and ($21),y              ; $8087: 31 21
	and $40,x                ; $8089: 35 40
	ror                      ; $808b: 6a
	cop #$1e                 ; $808c: 02 1e
	sbc $52,s                ; $808e: e3 52
	ora $2f,s                ; $8090: 03 2f
	ora $63,x                ; $8092: 15 63
	ply                      ; $8094: 7a
	ora ($22,s),y            ; $8095: 13 22
	ora $db10ef              ; $8097: 0f ef 10 db
	ora $64                  ; $809b: 05 64
	ply                      ; $809d: 7a
	and ($fe),y              ; $809e: 31 fe
	sbc $ccdd0e              ; $80a0: ef 0e dd cc
	cpx #$20                 ; $80a4: e0 20
	ror                      ; $80a6: 6a
	dec $ec0e,x              ; $80a7: de 0e ec
	cpy #$fc                 ; $80aa: c0 fc
	txs                      ; $80ac: 9a
	ldy $6ae0,x              ; $80ad: bc e0 6a
	tsb $cd9a                ; $80b0: 0c 9a cd
	cmp $24e0ed,x            ; $80b3: df ed e0 24
	bit $6a,x                ; $80b7: 34 6a
	rol $dece                ; $80b9: 2e ce de
	ora ($40,s),y            ; $80bc: 13 40
	inc $67f1,x              ; $80be: fe f1 67
	ply                      ; $80c1: 7a
	and ($02,x)              ; $80c2: 21 02
	and ($33,s),y            ; $80c4: 33 33
	and ($11)                ; $80c6: 32 11
	bpl loc_80cb             ; $80c8: 10 01
loc_80ca:
	ror                      ; $80ca: 6a
loc_80cb:
	inc $4304                ; $80cb: ee 04 43 A1B0
	jsl loc_fd5323           ; $80ce: 22 23 53 fd
loc_80d2:
	sbc $62156a              ; $80d2: ef 6a 15 62
	beq loc_80e7             ; $80d6: f0 0f
loc_80d8:
	cpy $1124                ; $80d8: cc 24 11
	and $c9dd6a              ; $80db: 2f 6a dd c9
	sta $ef1e77,x            ; $80df: 9f 77 1e ef
	sbc $6ade                ; $80e3: ed de 6a
	inc $e0dd,x              ; $80e6: fe dd e0
	jsr ($edce,x)            ; $80e9: fc ce ed
	dec $6a10,x              ; $80ec: de 10 6a
	brk #$0e                 ; $80ef: 00 0e

; === Block $8001-$8008 (Code) ===
loc_8001:
	bit $22,x                ; $8001: 34 22
	bmi $807f                ; $8003: 30 7a
loc_8005:
	jml [$6415]              ; $8005: dc 15 64

; === Block $8015-$801b (Code) ===
loc_8015:
	sbc $00ff7a              ; $8015: ef 7a ff 00
	ora ($0f)                ; $8019: 12 0f

; === Block $8047-$804a (Code) ===
loc_8047:
	inc $3502                ; $8047: ee 02 35

; === Block $808d-$8090 (Code) ===
loc_808d:
	asl $52e3,x              ; $808d: 1e e3 52

; === Block $80e7-$80ef (Code) ===
loc_80e7:
	cmp $fce0,x              ; $80e7: dd e0 fc
	dec $deed                ; $80ea: ce ed de
	bpl loc_8159             ; $80ed: 10 6a

; === Block $8159-$815d (Code) ===
loc_8159:
	sbc $1e436a,x            ; $8159: ff 6a 43 1e NTRL6
	brk #$00                 ; $815d: 00 00

