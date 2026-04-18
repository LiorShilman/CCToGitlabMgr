using System.Collections.Generic;

namespace CCToGitlabMgr.Views
{
    /// <summary>
    /// Hebrew help content for the migration guide.
    /// Technical Git terms are kept in English.
    /// </summary>
    public static class HelpContent
    {
        public class HelpChapter
        {
            public string Id { get; set; }
            public string Icon { get; set; }
            public string Title { get; set; }
            public string[] Paragraphs { get; set; }
        }

        public static List<HelpChapter> GetChapters()
        {
            return new List<HelpChapter>
            {
                new HelpChapter
                {
                    Id = "intro",
                    Icon = "\U0001F4D6",
                    Title = "מבוא — סקירה כללית",
                    Paragraphs = new[]
                    {
                        "כלי זה מנחה אותך בתהליך המעבר המלא מ-ClearCase ל-GitLab/GitHub, צעד אחר צעד.",

                        "התהליך כולל 9 שלבים מובנים:\n" +
                        "הגדרת Git במחשב, הכנת קוד המקור, ניקוי קבצים מיותרים, יצירת .gitignore,\n" +
                        "הקמת פרויקט ב-GitLab/GitHub, ביצוע ה-Migration עצמו, אימות התוצאה,\n" +
                        "רשימת בדיקות סופית, ולבסוף — כלים לעבודה יום-יומית עם Git.",

                        "חשוב — המעבר מ-ClearCase ל-Git הוא חד-כיווני.\n" +
                        "לאחר סיום התהליך, כל העבודה השוטפת תתבצע מול GitLab בלבד.\n" +
                        "ClearCase ישמש כארכיון לקריאה בלבד.",

                        "מבנה התיקיות בתהליך:\n" +
                        "• Source — תיקיית המקור מ-ClearCase (לקריאה בלבד, לא לשנות)\n" +
                        "• Staging — עותק עבודה זמני לביצוע ה-Migration\n" +
                        "• Verify — תיקיית Clone לאימות שהכל הועלה תקין\n" +
                        "• Working Directory — תיקיית הפיתוח היום-יומי (Clone טרי מ-GitLab/GitHub)"
                    }
                },

                new HelpChapter
                {
                    Id = "step1",
                    Icon = "01",
                    Title = "Git Config — הגדרת Git",
                    Paragraphs = new[]
                    {
                        "בשלב זה מגדירים את Git במחשב שלך.\n" +
                        "ההגדרות נשמרות ברמת Global ומשפיעות על כל הפרויקטים במחשב.",

                        "הגדרות חובה:\n" +
                        "• user.name — השם שלך כפי שיופיע בכל Commit\n" +
                        "• user.email — כתובת ה-Email שלך (חייבת להתאים לחשבון ה-GitLab/GitHub)",

                        "הגדרות מומלצות שהכלי מחיל אוטומטית:\n" +
                        "• core.autocrlf = true — המרה אוטומטית של סיומות שורה (Windows ↔ Linux)\n" +
                        "• init.defaultBranch = main — ה-Branch הראשי ייקרא main ולא master\n" +
                        "• core.longpaths = true — תמיכה בנתיבי קבצים ארוכים (חשוב ל-Windows)\n" +
                        "• pull.rebase = false — ביצוע Merge רגיל בעת Pull (בטוח יותר למתחילים)\n" +
                        "• core.bigFileThreshold = 50m — סף לזיהוי קבצים גדולים",

                        "לחצו \"Apply Recommended Config\" להחלת כל ההגדרות בלחיצה אחת.\n" +
                        "ניתן לראות את ההגדרות הנוכחיות בלחיצה על \"Show Current Config\"."
                    }
                },

                new HelpChapter
                {
                    Id = "step2",
                    Icon = "02",
                    Title = "Prepare — הכנת הפרויקט",
                    Paragraphs = new[]
                    {
                        "בשלב זה מעתיקים את קוד המקור מ-ClearCase לתיקיית Staging.",

                        "כלל הזהב:\n" +
                        "לעולם אל תבצעו git init ישירות בתיקיית ה-ClearCase!\n" +
                        "תמיד עבדו על עותק בתיקייה נפרדת.",

                        "מה לעשות:\n" +
                        "• בחרו את תיקיית Source — הנתיב לתיקייה ב-ClearCase\n" +
                        "• בחרו את תיקיית Staging — תיקייה ריקה שבה יתבצע התהליך\n" +
                        "• לחצו \"Copy to Staging\" — הכלי יעתיק את כל הקבצים",

                        "הערות:\n" +
                        "• תיקיית ה-Staging חייבת להיות ריקה לפני ההעתקה\n" +
                        "• תהליך ההעתקה עשוי לקחת זמן בפרויקטים גדולים\n" +
                        "• שם הפרויקט ואם קיים Solution — יזוהו אוטומטית"
                    }
                },

                new HelpChapter
                {
                    Id = "step3",
                    Icon = "03",
                    Title = "Clean — ניקוי קבצים",
                    Paragraphs = new[]
                    {
                        "שלב קריטי! לפני העלאה ל-Git חובה לנקות קבצים מיותרים.\n" +
                        "ב-ClearCase נשמרים קבצים רבים שאין להם מקום ב-Git.",

                        "ניקוי קבצי ClearCase:\n" +
                        "הכלי מזהה ומוחק אוטומטית קבצים ותיקיות של ClearCase:\n" +
                        "• .copyarea.db, .copyarea.dat — קבצי מטא-דאטה\n" +
                        "• lost+found — תיקיית שחזור של ClearCase\n" +
                        "• קבצים עם סיומת .contrib, .keep — קבצי Merge",

                        "ניקוי Build Artifacts:\n" +
                        "קבצים שנוצרים בזמן Build ואין לשמור אותם ב-Git:\n" +
                        "• תיקיות bin, obj, Debug, Release — תוצרי קומפילציה\n" +
                        "• קבצי .dll, .exe, .pdb — קבצים בינאריים\n" +
                        "• תיקיות packages, node_modules — תלויות שמותקנות מחדש\n" +
                        "• קבצי .suo, .user — הגדרות אישיות של Visual Studio",

                        "שמירה על תיקיות חשובות:\n" +
                        "לחצו \"Scan Preservable\" כדי לזהות תיקיות שאולי כדאי לשמר.\n" +
                        "לדוגמה, תיקיית bin שמכילה DLL-ים של צד שלישי שאינם זמינים ב-NuGet.\n" +
                        "סמנו את התיקיות שברצונכם לשמר לפני הניקוי.",

                        "Audit — בדיקת קבצים גדולים:\n" +
                        "לחצו \"Audit Files\" כדי לזהות קבצים שעלולים להיות גדולים מדי ל-Git.\n" +
                        "Git לא מתאים לקבצים בינאריים גדולים (מעל 50MB).\n" +
                        "שקלו להשתמש ב-Git LFS עבור קבצים כאלה."
                    }
                },

                new HelpChapter
                {
                    Id = "step4",
                    Icon = "04",
                    Title = "Gitignore — הגדרת חסימת קבצים",
                    Paragraphs = new[]
                    {
                        "קובץ .gitignore קובע אילו קבצים Git יתעלם מהם.\n" +
                        "זהו אחד הקבצים החשובים ביותר בכל פרויקט Git.",

                        "בחירת Template:\n" +
                        "הכלי מציע Templates מוכנים לפי סוג הפרויקט:\n" +
                        "• VisualStudio — מתאים לרוב פרויקטי C# ו-.NET\n" +
                        "• VisualStudio + Web — מוסיף חסימה של node_modules, bower וכו׳\n" +
                        "• Custom — ניתן להגדיר ידנית",

                        "מה נחסם אוטומטית:\n" +
                        "• תיקיות bin/ ו-obj/ — תוצרי Build\n" +
                        "• קבצי .suo, .user, .vs/ — הגדרות IDE אישיות\n" +
                        "• תיקיות packages/ — חבילות NuGet (משוחזרות ב-Restore)\n" +
                        "• קבצי *.log, *.tmp — קבצים זמניים",

                        "ניתן ללחוץ \"Preview .gitignore\" כדי לראות את תוכן הקובץ לפני ההנחה.\n" +
                        "לחצו \"Place .gitignore\" כדי ליצור את הקובץ בתיקיית ה-Staging.",

                        "טיפ חשוב:\n" +
                        "קובץ .gitignore חל רק על קבצים חדשים. קבצים שכבר נמצאים ב-Git\n" +
                        "לא ייחסמו אוטומטית — לכן חשוב להניח אותו לפני ה-Commit הראשון."
                    }
                },

                new HelpChapter
                {
                    Id = "step5",
                    Icon = "05",
                    Title = "Remote — הקמת פרויקט",
                    Paragraphs = new[]
                    {
                        "בשלב זה מגדירים את הפרויקט ב-GitLab/GitHub שיקבל את הקוד.",

                        "יצירת פרויקט ב-GitLab/GitHub:\n" +
                        "• היכנסו ל-GitLab/GitHub דרך הדפדפן\n" +
                        "• צרו פרויקט חדש (New Project → Create Blank Project)\n" +
                        "• חשוב: בטלו את \"Initialize repository with a README\"\n" +
                        "  (ה-Repository חייב להיות ריק לחלוטין)",

                        "הגדרת Authentication:\n" +
                        "יש שתי שיטות התחברות ל-GitLab/GitHub:\n\n" +
                        "SSH Key (מומלץ):\n" +
                        "• מאובטח יותר, לא דורש הקלדת סיסמה בכל Push\n" +
                        "• הכלי יכול לייצר SSH Key עבורכם\n" +
                        "• יש להעתיק את ה-Public Key ל-GitLab/GitHub → Settings → SSH Keys\n\n" +
                        "Personal Access Token:\n" +
                        "• פשוט יותר להגדרה\n" +
                        "• צרו Token ב-GitLab/GitHub → Settings → Access Tokens\n" +
                        "• הרשאות נדרשות: read_repository, write_repository\n" +
                        "• ה-Token מוטמע ב-URL של ה-Remote",

                        "Remote URL:\n" +
                        "הכתובת שבה Git ישתמש לתקשורת עם GitLab.\n" +
                        "• עבור SSH: git@gitlab.example.com:group/project.git\n" +
                        "• עבור HTTPS: https://gitlab.example.com/group/project.git\n" +
                        "• עבור Token: https://oauth2:TOKEN@gitlab.example.com/group/project.git"
                    }
                },

                new HelpChapter
                {
                    Id = "step6",
                    Icon = "06",
                    Title = "Migrate — ביצוע ההעברה",
                    Paragraphs = new[]
                    {
                        "זהו השלב המרכזי — כאן מתבצעת ההעברה בפועל מ-Staging ל-GitLab/GitHub.",

                        "מה קורה בתהליך:\n" +
                        "הכלי מבצע אוטומטית את הצעדים הבאים:\n" +
                        "1. git init — אתחול Repository חדש בתיקיית ה-Staging\n" +
                        "2. git branch -M main — הגדרת ה-Branch הראשי\n" +
                        "3. git add . — הוספת כל הקבצים ל-Staging Area\n" +
                        "4. git commit — יצירת ה-Commit הראשון עם ההודעה שכתבתם\n" +
                        "5. git remote add origin — הגדרת הקישור ל-GitLab/GitHub\n" +
                        "6. git push -u origin main — העלאת הקוד ל-GitLab/GitHub",

                        "לפני ביצוע:\n" +
                        "• ודאו שכל השלבים הקודמים הושלמו בהצלחה\n" +
                        "• ודאו שתיקיית ה-Staging נקייה מקבצים מיותרים\n" +
                        "• כתבו הודעת Commit ברורה ומתארת\n" +
                        "• לחצו \"Dry Run Check\" לבדיקה מקדימה ללא ביצוע",

                        "Dry Run Check:\n" +
                        "בודק את מספר הקבצים שיועלו מבלי לבצע את ה-Migration.\n" +
                        "מומלץ להריץ לפני כל Migration כדי לוודא שהכמות הגיונית.",

                        "הודעת Commit:\n" +
                        "כתבו הודעה ברורה, לדוגמה:\n" +
                        "\"Initial migration from ClearCase to GitLab\"\n" +
                        "ההודעה תשמר לנצח בהיסטוריית הפרויקט."
                    }
                },

                new HelpChapter
                {
                    Id = "step7",
                    Icon = "07",
                    Title = "Verify — אימות ההעברה",
                    Paragraphs = new[]
                    {
                        "אחרי ההעברה חובה לוודא שהכל הועלה תקין ל-GitLab/GitHub.",

                        "מה עושים:\n" +
                        "• בחרו תיקייה ריקה חדשה עבור ה-Verify\n" +
                        "• לחצו \"Clone & Verify\" — הכלי יוריד Clone טרי מ-GitLab/GitHub\n" +
                        "• הכלי ישווה בין מספר הקבצים ב-Staging לבין ה-Clone\n" +
                        "• פתחו את הפרויקט ב-Visual Studio ונסו לעשות Build",

                        "מה לבדוק ידנית:\n" +
                        "• פתחו את ה-Solution ב-Visual Studio\n" +
                        "• בצעו NuGet Restore ואחריו Build\n" +
                        "• ודאו שאין שגיאות קומפילציה\n" +
                        "• הריצו את ה-Unit Tests אם קיימים\n" +
                        "• בדקו שקבצי Configuration קיימים (web.config, app.config)",

                        "אם ה-Build נכשל:\n" +
                        "• חסר Reference — ייתכן שקובץ DLL נמחק בשלב הניקוי.\n" +
                        "  חיזרו לשלב 3 ובדקו את \"Scan Preservable\"\n" +
                        "• חסר NuGet Package — הריצו NuGet Restore\n" +
                        "• שגיאת נתיב — בדקו core.longpaths בשלב 1",

                        "README:\n" +
                        "הכלי יכול לייצר קובץ README.md אוטומטית עם פרטי הפרויקט.\n" +
                        "ניתן לערוך ולשמור לתיקיית ה-Staging לפני Push נוסף."
                    }
                },

                new HelpChapter
                {
                    Id = "step8",
                    Icon = "08",
                    Title = "Checklist — רשימת בדיקות",
                    Paragraphs = new[]
                    {
                        "רשימת בדיקות מסכמת לפני שמכריזים על סיום ה-Migration.",

                        "בדיקות לפני Migration:\n" +
                        "• ✓ Git מותקן ומוגדר עם שם ו-Email\n" +
                        "• ✓ הקבצים הועתקו ל-Staging\n" +
                        "• ✓ קבצי ClearCase נוקו\n" +
                        "• ✓ Build Artifacts נוקו\n" +
                        "• ✓ קובץ .gitignore הונח",

                        "בדיקות אחרי Migration:\n" +
                        "• ✓ ה-Push ל-GitLab/GitHub הצליח\n" +
                        "• ✓ Clone חדש מ-GitLab/GitHub עובד\n" +
                        "• ✓ ה-Build עובר בהצלחה\n" +
                        "• ✓ Tests עוברים\n" +
                        "• ✓ כל הקבצים הנדרשים קיימים",

                        "בדיקות גישה וצוות:\n" +
                        "• ✓ חברי הצוות יכולים לגשת לפרויקט ב-GitLab/GitHub\n" +
                        "• ✓ הרשאות Push / Pull מוגדרות נכון\n" +
                        "• ✓ Branch Protection מוגדר ל-main (אופציונלי)\n" +
                        "• ✓ ה-CI/CD Pipeline מוגדר (אם רלוונטי)",

                        "סמנו כל פריט ברשימה כ-Done לאחר ביצועו.\n" +
                        "כשכל הפריטים מסומנים — ה-Migration הושלם בהצלחה!"
                    }
                },

                new HelpChapter
                {
                    Id = "step9",
                    Icon = "09",
                    Title = "Daily Workflow — עבודה יום-יומית",
                    Paragraphs = new[]
                    {
                        "לאחר סיום ה-Migration, שלב 9 מספק כלים לעבודה שוטפת עם Git.",

                        "Working Directory:\n" +
                        "תיקיית העבודה היום-יומית נפרדת מתיקיית ה-Staging!\n" +
                        "בחרו תיקייה חדשה ולחצו \"Clone from GitLab\" להורדת עותק טרי.\n" +
                        "כל העבודה השוטפת תתבצע בתיקייה זו.",

                        "הזרימה היום-יומית:\n" +
                        "1. Fetch — בדיקה מה חדש ב-Remote (בלי למזג)\n" +
                        "2. Pull — קבלת השינויים האחרונים מ-GitLab/GitHub ומיזוגם\n" +
                        "   תמיד התחילו את יום העבודה ב-Pull!\n" +
                        "3. עבודה על הקוד — כתבו, תקנו, שפרו\n" +
                        "4. Stage + Commit — שמירת תמונת מצב עם הודעה מתארת\n" +
                        "   כתבו הודעות Commit קצרות וברורות באנגלית\n" +
                        "   לדוגמה: \"Fix login timeout bug\" או \"Add CSV export feature\"\n" +
                        "5. Push — העלאת השינויים ל-GitLab/GitHub\n" +
                        "   כך חברי הצוות יוכלו לראות את העבודה שלכם",

                        "Smart Pull:\n" +
                        "אם יש לכם שינויים מקומיים שלא עשיתם להם Commit,\n" +
                        "הכלי יזהה זאת ויציע לבצע Stash → Pull → Stash Pop אוטומטית.\n" +
                        "זה מגן על העבודה שלכם ומונע קונפליקטים.",

                        "Undo / Reset — ביטול פעולות:\n" +
                        "• Unstage All — מוציא קבצים מה-Staging Area, השינויים נשמרים\n" +
                        "• Undo Last Commit — מבטל את ה-Commit האחרון, הקבצים נשארים staged\n" +
                        "• Discard All Changes — מוחק את כל השינויים שלא עשיתם להם Commit!\n" +
                        "  זהירות: פעולה זו בלתי הפיכה!\n" +
                        "• Amend Last Commit — מתקן את הודעת ה-Commit האחרון או מוסיף קבצים ששכחתם",

                        "Fetch לעומת Pull:\n" +
                        "• Fetch — מביא מידע מהשרת בלי לשנות את הקבצים המקומיים.\n" +
                        "  שימושי לבדיקה אם יש שינויים חדשים לפני Pull.\n" +
                        "• Pull — מבצע Fetch ואחריו Merge, כלומר מעדכן את הקבצים בפועל.",

                        "טיפים לעבודה נכונה:\n" +
                        "• עשו Commit לעיתים קרובות — Commits קטנים וממוקדים עדיפים\n" +
                        "• כתבו הודעות Commit מתארות — חברי הצוות (ואתם בעתיד) יודו לכם\n" +
                        "• עשו Push בסוף כל יום עבודה — גיבוי ושיתוף\n" +
                        "• השתמשו ב-Branches לפיצ'רים שלוקחים יותר מיום"
                    }
                },

                new HelpChapter
                {
                    Id = "branches",
                    Icon = "\U0001F500",
                    Title = "Branches — ענפי פיתוח",
                    Paragraphs = new[]
                    {
                        "Branches הם אחד הכלים החזקים ביותר ב-Git.\n" +
                        "הם מאפשרים לעבוד על פיצ'רים או תיקונים בנפרד מהקוד הראשי.",

                        "מתי ליצור Branch:\n" +
                        "• פיצ'ר חדש שלוקח יותר מיום אחד\n" +
                        "• תיקון באג מורכב שדורש מספר Commits\n" +
                        "• ניסוי או Proof of Concept\n" +
                        "• כשרוצים לעבוד בלי להשפיע על main",

                        "מוסכמות שמות:\n" +
                        "• feature/add-export — לפיצ'ר חדש\n" +
                        "• bugfix/login-timeout — לתיקון באג\n" +
                        "• hotfix/critical-fix — לתיקון דחוף ב-Production\n" +
                        "• refactor/cleanup-auth — לשיפור קוד קיים",

                        "זרימת עבודה עם Branches:\n" +
                        "1. צרו Branch חדש מ-main (\"Create & Switch\")\n" +
                        "2. עבדו ועשו Commits ב-Branch\n" +
                        "3. עשו Push ל-GitLab/GitHub\n" +
                        "4. פתחו Merge/Pull Request ב-GitLab/GitHub\n" +
                        "5. אחרי Code Review — מבצעים Merge ל-main\n" +
                        "6. מוחקים את ה-Branch המקומי (\"Delete Branch\")",

                        "ניהול Branches בכלי:\n" +
                        "• Create & Switch — יוצר Branch חדש ועובר אליו\n" +
                        "• Switch to Branch — מעבר ל-Branch קיים לפי שם\n" +
                        "• Switch to main — חזרה מהירה ל-Branch הראשי\n" +
                        "• Delete Branch — מחיקת Branch מקומי (רק אחרי Merge)\n" +
                        "• List Branches — תצוגה מעוצבת של כל ה-Branches\n" +
                        "• Branch Graph — תצוגת עץ ויזואלית של ההיסטוריה",

                        "Merge/Pull Request (MR):\n" +
                        "ב-GitLab/GitHub, Merge/Pull Request הוא הדרך המומלצת למזג שינויים.\n" +
                        "הוא מאפשר Code Review, דיון, והפעלת CI/CD לפני המיזוג.\n" +
                        "תמיד השתמשו ב-MR במקום Merge ישיר ל-main."
                    }
                },

                new HelpChapter
                {
                    Id = "tags",
                    Icon = "\U0001F3F7",
                    Title = "Tags — תגיות גרסה",
                    Paragraphs = new[]
                    {
                        "Tags ב-Git משמשים לסימון נקודות חשובות בהיסטוריה,\n" +
                        "בדרך כלל שחרורי גרסאות (Releases).",

                        "סוגי Tags:\n" +
                        "• Lightweight Tag — סימון פשוט, ללא מידע נוסף\n" +
                        "• Annotated Tag — כולל הודעה, תאריך, ושם היוצר (מומלץ)",

                        "מוסכמות שמות גרסאות (Semantic Versioning):\n" +
                        "• v1.0.0 — גרסה ראשונית\n" +
                        "• v1.1.0 — תוספת פיצ'ר (Minor)\n" +
                        "• v1.1.1 — תיקון באג (Patch)\n" +
                        "• v2.0.0 — שינוי משמעותי שאינו תואם אחורה (Major)",

                        "שימוש ב-Tags:\n" +
                        "• צרו Tag אחרי כל Release\n" +
                        "• כתבו הודעה מתארת את מה שנכלל ב-Release\n" +
                        "• עשו Push Tags ל-GitLab/GitHub כדי שיופיעו ב-UI\n" +
                        "• ב-GitLab/GitHub ניתן ליצור Release Notes מ-Tags"
                    }
                },

                new HelpChapter
                {
                    Id = "glossary",
                    Icon = "\U0001F4DA",
                    Title = "מילון מושגים",
                    Paragraphs = new[]
                    {
                        "Repository (Repo)\n" +
                        "מאגר קוד שמנוהל על ידי Git. כולל את כל הקבצים, ההיסטוריה, וה-Branches.",

                        "Commit\n" +
                        "תמונת מצב (Snapshot) של כל הקבצים בנקודת זמן מסוימת.\n" +
                        "כל Commit כולל הודעה מתארת, תאריך, ומזהה ייחודי (Hash).",

                        "Branch\n" +
                        "ענף פיתוח נפרד. מאפשר עבודה מקבילה על פיצ'רים שונים.\n" +
                        "ה-Branch הראשי נקרא main.",

                        "Merge\n" +
                        "מיזוג שינויים מ-Branch אחד לאחר.\n" +
                        "ב-GitLab/GitHub מתבצע דרך Merge/Pull Request.",

                        "Clone\n" +
                        "הורדת עותק מלא של ה-Repository מהשרת למחשב המקומי.\n" +
                        "כולל את כל ההיסטוריה וה-Branches.",

                        "Pull\n" +
                        "עדכון ה-Repository המקומי מהשרת (GitLab).\n" +
                        "מביא שינויים שבוצעו על ידי חברי צוות אחרים.",

                        "Push\n" +
                        "העלאת ה-Commits המקומיים לשרת (GitLab).\n" +
                        "הופך את השינויים שלכם לזמינים לכולם.",

                        "Staging Area (Index)\n" +
                        "אזור ביניים שבו מסמנים אילו שינויים ייכללו ב-Commit הבא.\n" +
                        "git add מוסיף קבצים ל-Staging Area.",

                        "Stash\n" +
                        "שמירה זמנית של שינויים שלא עשיתם להם Commit.\n" +
                        "שימושי כשצריך לעבור Branch בלי לאבד עבודה.",

                        "Tag\n" +
                        "תגית שמסמנת נקודה ספציפית בהיסטוריה.\n" +
                        "משמש בעיקר לסימון גרסאות (v1.0.0, v2.1.3).",

                        ".gitignore\n" +
                        "קובץ שמגדיר אילו קבצים ותיקיות Git יתעלם מהם.\n" +
                        "לדוגמה: bin/, obj/, *.suo, node_modules/.",

                        "Remote\n" +
                        "שרת מרוחק שמאחסן את ה-Repository.\n" +
                        "ברירת מחדל: origin (מצביע ל-GitLab/GitHub).",

                        "Conflict\n" +
                        "מצב שבו שני אנשים שינו את אותו קטע קוד.\n" +
                        "Git לא יכול להחליט איזה שינוי לשמור — נדרש פתרון ידני.",

                        "Fetch\n" +
                        "הורדת מידע מהשרת (GitLab) בלי לעדכן את הקבצים המקומיים.\n" +
                        "מאפשר לבדוק מה חדש לפני שעושים Pull.",

                        "Amend\n" +
                        "תיקון ה-Commit האחרון — שינוי ההודעה או הוספת קבצים ששכחתם.\n" +
                        "חשוב: אל תעשו Amend ל-Commits שכבר עשיתם להם Push!",

                        "Reset\n" +
                        "ביטול פעולות ב-Git.\n" +
                        "• reset --soft — מבטל Commit, שומר שינויים staged\n" +
                        "• checkout -- . — מוחק שינויים ומחזיר לגרסה האחרונה",

                        "Diff\n" +
                        "השוואה בין שתי גרסאות של קובץ.\n" +
                        "שורות ירוקות (+) = הוספה, שורות אדומות (-) = מחיקה."
                    }
                }
            };
        }
    }
}
