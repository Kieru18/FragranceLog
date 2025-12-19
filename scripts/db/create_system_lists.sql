INSERT INTO PerfumeList (UserId, Name, IsSystem, CreationDate)
SELECT u.UserId, 'Owned', 1, GETDATE()
FROM Users u
WHERE u.Username NOT LIKE 'SystemUser_%'
  AND NOT EXISTS (
      SELECT 1
      FROM PerfumeList pl
      WHERE pl.UserId = u.UserId
        AND pl.Name = 'Owned'
  );

INSERT INTO PerfumeList (UserId, Name, IsSystem, CreationDate)
SELECT u.UserId, 'Wanted', 1, GETDATE()
FROM Users u
WHERE u.Username NOT LIKE 'SystemUser_%'
  AND NOT EXISTS (
      SELECT 1
      FROM PerfumeList pl
      WHERE pl.UserId = u.UserId
        AND pl.Name = 'Wanted'
  );
