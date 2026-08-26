# Ocean Battle Royale

Tarayıcıda çalışan, agar.io × battle royale karışımı 3D deniz savaşı oyunu.
Unity yerine **Three.js** ile yazıldı — kurulum yok, indirme yok, direkt oyna.

**Oyna:** https://turkerpro.github.io/ocean-battle-royale/

## Oyun Mekanikleri
- **5 gemi katı:** Sandal → Tekne → Korvet → Firkateyn → Zırhlı (her biri farklı model + top sayısı)
- **Sandık toplama:** Altın sandık XP, yeşil sandık can verir (agar.io çekirdek döngüsü)
- **Daralan bölge:** Klasik BR — dışarıda kalırsan hasar alırsın
- **29 bot:** Durum makinesi AI (gezinme / saldırı / kaçış), sandık toplar, seviye atlar, mayın döşer
- **Mayın:** Düşman yaklaşınca patlar, ölümlü
- **Zafer koşulu:** Son hayatta kalan gemi kazanır

## Teknik
- Tek dosya: `index.html` (Three.js r128 CDN)
- Custom GLSL su shader'ı (dalga + köpük + parıltı), gemiler dalgalara göre zıplar
- Web Audio API ile sentezlenmiş ses efektleri (asset yok)
- Canvas sprite hasar sayıları, billboard can barları, ekran sarsıntısı, izler
- Highscore `localStorage`'da

## Kontroller
| Tuş | İşlev |
|---|---|
| WASD / Oklar | Dümen + gaz |
| Fare + Sol Tık | Nişan + ateş |
| SPACE | Mayın döşe |
| 1-5 | Açılmış gemi katına geç |
| Fare Tekerleği | Zoom |
| P | Duraklat |

## Deploy
`main`'e push → GitHub Actions statik deploy → GitHub Pages. Başka hiçbir şey gerekmez.
