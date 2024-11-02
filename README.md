# AMOS ASCOM Dome Driver

for use with Astrometric ProDomeU controller without permanent power for cover control

- always rotate to park position before shutter operation
- fake shutter status during operation
- fake AtHome and AtPark, they will be true as soon as the cover control unit has power (which is usualle the park position)

## Known issue

- Shutter status will be wrong if stopped intermediately