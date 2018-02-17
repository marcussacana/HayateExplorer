# HayateExplorer

Tool to extract the files of the "Hayate no Gotoku" psp game.



# Formats

All files in the game packget don't have extension or name and is automatically generated, i will list what i know of the game files

- **sce:** Game Scripts, The text use a unknown encoding, isn't ecrypted, the first script after the new game is the **0119E800.sce**
- **bin:** Small Game packget
- **hed:**  **lba** packget header, to know what is the lba of the .hed after all offsets have the .lba file length
- **lba:** Bigger Game packget without header, the **.hed** files in the **.bin** is the packget header

# Encoding
I don't know mutch about the game encoding, but here have a small useless sample list:
```
8000= 
8001=０
8002=１
8003=２
8004=３
8005=４
8006=５
8007=６
8008=７
8009=８
800A=９
800B='
800C="
800D=、
800E=。
800F=々
8010=〉
8011=》
8012=」
8013=』
8014=】
8015=〕
8018=！
8019=％
801A=）
801B=，
801C=．
801D=：
801E=；
801F=？
8020=］
8021=｝
8022=…
8023=～
8024=ぁ
8025=ぃ
80C0=ウ
80EC=フ
```