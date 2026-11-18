import sqlite3
conn = sqlite3.connect('swms_local_dev.db')
c = conn.cursor()
c.execute("PRAGMA table_info(lessons)")
print("LESSONS COLS:", [r[1] for r in c.fetchall()])
c.execute("SELECT id, email FROM users WHERE email LIKE '%son.tran%'")
users = c.fetchall()
print('Users:', users)
if users:
    uid = users[0][0]
    c.execute("SELECT course_id FROM course_enrollments WHERE user_id = ?", (uid,))
print('Enrolled courses for Son Tran:', [r[0] for r in c.fetchall()])

