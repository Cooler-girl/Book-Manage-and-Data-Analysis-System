using BookMS.Models;
using System.Collections.ObjectModel;

namespace BookMS.Data
{
    public class DbInitializer
    {
        public static void Initialize(BookContext context)
        {
            context.Database.EnsureCreated();

            if (context.Books.Any())
            {
                return;
            }
            var b1 = new Book { 
                BookID = "GS1001197546", Title = "永远讲不完的故事", 
                Img = "images/book/永远讲不完的故事.jpg", Type = "故事", 
                Words = 200000, 
                Introduce = "小说主人公巴斯蒂安·巴尔塔沙·布克斯是一个胖胖的、行动笨拙、经常被嘲笑、被愚弄的、不快乐的男孩。有一天，他从一家书店偷了一本书，书名为《永远讲不完的故事》，书中是一个神奇的王国，幻想王国正在毁灭，天真女皇生命垂危，只有一个人间的小孩为她起一个新的名字，她和幻想王国方能得救。。。", 
                Price = 30.00, 
                Count = 200, 
                PubDate = DateTime.Parse("2009-03-01") 
            };
            var b2 = new Book { 
                BookID = "GS7355194994", 
                Title = "当世界年纪还小的时候", 
                Img = "images/book/当世界年纪还小的时候.jpg", 
                Type = "故事", 
                Words = 18000, 
                Introduce= "天堂是这样一个世界——也就是当她还小的时候的那个世界。随着时间的流逝，一切都改变了。 接下来的其实是大家所熟知的，但是舒比格让人惊喜、充满童话色彩的故事是不会让你提前猜到结局的，要耐心等待。",
                Price = 28.00, 
                Count = 100, 
                PubDate = DateTime.Parse("2006-10-01")
            };
            var b3 = new Book {
                BookID = "RW2052180980",
                Title = "我的狼妈妈",
                Img = "images/book/我的狼妈妈.jpg",
                Type = "人物", 
                Words = 32000,
                Introduce = "就在那一刹那，我的身体感受到一股巨大的冲击力，猛地被推到了一边。倒在地上的瞬间，我听见越野车刺耳的刹车声，还听见砰的一声巨响。",
                Price = 17.00, 
                Count = 100, 
                PubDate = DateTime.Parse("2012-06-01") 
            };
            var b4 = new Book { 
                BookID = "GS8769724205", 
                Title = "向着明亮那方", 
                Img = "images/book/向着明亮那方.jpg", 
                Type = "人物", 
                Words = 17500, 
                Introduce= "真趣美如天籁的童谣经典。2007年初，新星出版社推出了金子美铃童谣集《向着明亮那方》，这是金子美铃的童谣首次在中国大陆出版。本书精选金子美铃童诗187首，分为“夏”、“秋”、“春”、“冬”、“心”、“梦”六卷，并配有精美温馨的彩绘插图。",
                Price = 28.00, 
                Count = 100, 
                PubDate = DateTime.Parse("2009-01-01") 
            };
            var b5 = new Book { 
                BookID = "WP8479320936", 
                Title = "一百条裙子", 
                Img = "images/book/一百条裙子.jpg", 
                Type = "人物",
                Words = 40000, 
                Introduce= "旺达·佩特罗斯基是一个有着奇怪姓氏的女孩。也正是因为她的怪名字和旧裙子，所有的女生都喜欢捉弄她。直到有一天，旺达突然声称她家里有一百条各式各样的裙子，随之而来的却是更多的嘲笑，根本没有人相信她",
                Price = 12.00, 
                Count = 100, 
                PubDate = DateTime.Parse("2009-12-17") 
            };
            var books = new Book[]
            {
                b1,b2,b3,b4,b5
            };
            foreach (var book in books)
            {
                context.Books.Add(book);
            }
            context.SaveChanges();

            if (context.Authors.Any())
            {
                return;
            }
            var books1 = new Collection<Book>();
            books1.Add(b1);
            books1.Add(b5);
            var books2 = new Collection<Book>();
            books2.Add(b2);
            var books3 = new Collection<Book>();
            books3.Add(b3);
            var books4 = new Collection<Book>();
            books4.Add(b4);
            var authors = new Author[]
            {
                new Author{
                    Name="曹文轩",
                    Img="images/author/曹文轩.jpg",
                    Sex='男',
                    Country="中国",
                    Nation="汉族",
                    BirthPlace="江苏盐城",
                    BirthYear=DateTime.Parse("1954-01-01"),
                    Introduce="曹文轩，1954年1月出生于江苏盐城市盐都区学富镇中兴街道周伙村，中国作家、北京大学教授、中国作家协会全国委员会委员、北京作家协会副主席、儿童文学委员会委员、中国作家协会鲁迅文学院客座教授、中国作家协会儿童文学委员会主任",
                    Books=books1
                },
                new Author{
                    Name="孙幼军",
                    Img="images/author/孙幼军.jpg",
                    Sex='男',
                    Country="中国",
                    Nation="汉族",
                    BirthPlace="哈尔滨",
                    BirthYear=DateTime.Parse("1933-05-20"),
                    Introduce="孙幼军（1933年5月——2015年8月6日），男，出生于哈尔滨。1954年考入北京俄专二部，1955年入北京大学中文系。1960年毕业分配到外交学院执教。孙幼军被誉为“一代童话大师”，是著名童话作家、中国首位安徒生奖提名者、《小布头奇遇记》作者",
                    Books=books2},
                new Author{
                    Name="梅子涵",
                    Img="images/author/梅子涵.jpg",
                    Sex='男',Country="中国",
                    Nation="汉族",
                    BirthPlace="上海",
                    BirthYear=DateTime.Parse("1949-01-01"),
                    Introduce="20世纪70年代末期开始发表文学作品。 作为儿童文学家，他为儿童写了几十部书集，如《女儿的故事》、《戴小桥和他的哥们儿》等；作为儿童文学的研究者，他写作、主编了多部理论著作，如《儿童小说叙事式论》等",
                    Books=books3
                },
                new Author{
                    Name="周锐",
                    Img="images/author/周锐.jpg",
                    Sex='男',
                    Country="中国",
                    Nation="汉族",
                    BirthPlace="江苏南京",
                    BirthYear=DateTime.Parse("1953-01-01"),
                    Introduce="周锐是目 前国内最优秀的童话作家之一。自80年代中期，他已经出版了近80本书。在上海读到初中，先后去云南和苏北务农6年。1974年被推荐至南京河运学校学习轮机专业，毕业后在长江油轮上当轮机工。此时开始向刊物投稿。写过诗歌、小说、散文等，1979年起逐步转向儿童文学。1987年调回上海，在上海钢铁一厂的运输船队当驳船水手。1989年偶然争取到市领导的支持，调入上海人民美术出版社任编辑。",
                    Books=books4
                },
                new Author{
                    Name="沈石溪",
                    Img="images/author/沈石溪.jpg",
                    Sex='男',Country="中国",Nation="汉族",
                    BirthPlace="上海",BirthYear=DateTime.Parse("1952-10-12"),
                    Introduce="沈石溪，本名沈一鸣，1952年10月出生于上海亭子间，祖籍浙江慈溪，中国当代动物小说作家。现任中国作家协会儿童文学委员会委员，上海作家协会理事。"
                },
                new Author{
                    Name="秦文君",
                    Img="images/author/秦文君.jpg",
                    Sex='女',
                    Country="中国",
                    Nation="汉族",
                    BirthPlace="上海",
                    BirthYear=DateTime.Parse("1954-01-01"),
                    Introduce="儿童文学作家，现任上海市作家协会副主席，中国作家协会全委会会员，《中国儿童文学》主编，中国作家协会儿童文学委员会副主任。出版作品600多万字，其中包括《男生贾里全传》《女生贾梅全传》《宝贝当家》《一个女孩的心灵史》《调皮的日子》《逃逃》《小丫林晓梅》《小香咕系列》和《会跳舞的向日葵》等50多部作品。作品先后50余次获得国内外大奖、10余次被改编成影视剧，10多篇作品被收入中小学语文课本，还有10多部作品出版了日文版、英文版、德文版、韩文版、荷兰文版等发行到海外。2017年11月16日，获得2017陈伯吹国际儿童文学奖年度作家奖。"
                },
                new Author
                {
                    Name="徐玲",
                    Img="images/author/徐玲.jpg",
                    Sex='女',
                    Country="中国",
                    Nation="汉族",
                    BirthPlace="江苏",
                    BirthYear=DateTime.Parse("1964-05-20"),
                    Introduce="徐玲，现为中国作家协会会员、江苏省作家协会签约作家、希望出版社签约作家、中国作协与中国日报合作向海外推介优秀作家。著有长篇小说《流动的花朵》《长大后我想成为你》等。"
                }

            };
            foreach (var author in authors)
            {
                context.Authors.Add(author);
            }
            context.SaveChanges();
        }
    }
}
