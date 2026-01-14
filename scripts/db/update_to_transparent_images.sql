UPDATE PerfumePhotos
SET Path =
    LEFT(Path, LEN(Path) - CHARINDEX('.', REVERSE(Path))) + '.png'
WHERE Path IS NOT NULL
  AND (
        Path LIKE '%.jpg'
     OR Path LIKE '%.jpeg'
  );
