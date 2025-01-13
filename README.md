# Beton Brutal map utilities

These utilities are meant to convert Beton Brutal maps to a human-readable text format and vice versa to simplify editing. I don't plan to develop this further, yet I might make some improvements in the future (especially if related to some newer map versions).

Please note that you should save proper text formatting when using BBMapWriter.

Usage:
```
BBMapReader <map file name> [output file name]
BBMapWriter <map file name> [output file name]
```

Plans:
- possibly merge these two programs into one
- improve the writer (make it less reliant on formatting)
- backup an existing file in case of a conflict (currently overwrites)