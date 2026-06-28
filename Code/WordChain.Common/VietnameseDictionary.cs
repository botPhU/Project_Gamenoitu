using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace WordChain.Common
{
    /// <summary>
    /// Từ điển tiếng Việt dùng cho game nối từ — chỉ chấp nhận cụm đúng 2 từ.
    /// Danh sách được tuyển chọn từ từ vựng tiếng Việt phổ thông (THPT, đời sống hằng ngày).
    /// </summary>
    public static class VietnameseDictionary
    {
        private static readonly HashSet<string> _tuHaiTu = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Random _random = new();

        static VietnameseDictionary()
        {
            foreach (string tu in DanhSachTuHaiTuMacDinh())
            {
                string chuanHoa = ChuanHoaCumTu(tu);
                if (!string.IsNullOrWhiteSpace(chuanHoa))
                {
                    _tuHaiTu.Add(chuanHoa);
                }
            }
        }

        public static int SoLuongTu => _tuHaiTu.Count;

        public static string ChuanHoaCumTu(string cumTu)
        {
            if (string.IsNullOrWhiteSpace(cumTu))
            {
                return string.Empty;
            }

            string[] parts = cumTu.Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
            {
                return string.Empty;
            }

            return string.Join(' ',
                parts.Select(p => p.ToLower(CultureInfo.GetCultureInfo("vi-VN"))));
        }

        public static int DemSoTu(string cumTu)
        {
            if (string.IsNullOrWhiteSpace(cumTu))
            {
                return 0;
            }

            return cumTu.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public static bool LaDungHaiTu(string cumTu) => DemSoTu(cumTu) == 2;

        public static string LayAmTietCuoi(string cumTu)
        {
            string[] tu = cumTu.Trim()
                .ToLower(CultureInfo.GetCultureInfo("vi-VN"))
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return tu.Length == 0 ? string.Empty : tu[^1];
        }

        public static string LayAmTietDau(string cumTu)
        {
            string[] tu = cumTu.Trim()
                .ToLower(CultureInfo.GetCultureInfo("vi-VN"))
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return tu.Length == 0 ? string.Empty : tu[0];
        }

        public static bool CoTrongTuDien(string cumTu)
        {
            string chuanHoa = ChuanHoaCumTu(cumTu);
            return !string.IsNullOrWhiteSpace(chuanHoa) && _tuHaiTu.Contains(chuanHoa);
        }

        public static bool KiemTraNoiTu(string tuHienTai, string tuMoi)
        {
            string amTietCuoi = LayAmTietCuoi(tuHienTai);
            string amTietDau = LayAmTietDau(tuMoi);

            return !string.IsNullOrWhiteSpace(amTietCuoi) &&
                   string.Equals(amTietCuoi, amTietDau, StringComparison.OrdinalIgnoreCase);
        }

        public static (bool HopLe, string ThongBao) KiemTraTuMoi(
            string tuHienTai, string tuMoi, ISet<string> tuDaDung)
        {
            if (!LaDungHaiTu(tuMoi))
            {
                int soTu = DemSoTu(tuMoi);
                if (soTu < 2)
                {
                    return (false, "Từ phải gồm đúng 2 tiếng (ví dụ: con mèo, cây tre).");
                }

                return (false, "Chỉ được nhập cụm 2 từ. Cụm 3 từ trở lên không hợp lệ.");
            }

            string chuanHoa = ChuanHoaCumTu(tuMoi);

            if (!KiemTraNoiTu(tuHienTai, chuanHoa))
            {
                return (false,
                    $"Từ mới phải bắt đầu bằng \"{LayAmTietCuoi(tuHienTai)}\".");
            }

            if (tuDaDung.Contains(chuanHoa))
            {
                return (false, "Từ này đã được dùng trong ván chơi.");
            }

            if (!CoTrongTuDien(chuanHoa))
            {
                return (false, "Từ không có trong từ điển tiếng Việt.");
            }

            return (true, string.Empty);
        }

        public static string LayTuNgauNhien()
        {
            if (_tuHaiTu.Count == 0)
            {
                return "con mèo";
            }

            int index = _random.Next(_tuHaiTu.Count);
            return _tuHaiTu.ElementAt(index);
        }

        private static IEnumerable<string> DanhSachTuHaiTuMacDinh()
        {
            return
            [
                "con mèo", "con chó", "con gà", "con bò", "con lợn", "con vịt", "con cá",
                "con chim", "con ong", "con kiến", "con muỗi", "con ruồi", "con dê",
                "con ngựa", "con trâu", "con hổ", "con hươu", "con nai", "con voi",
                "con khỉ", "con thỏ", "con chuột", "con rắn", "con ếch", "con cua",
                "cây tre", "cây chuối", "cây dừa", "cây cam", "cây bưởi", "cây xoài",
                "cây nhãn", "cây vải", "cây na", "cây mít", "cây đu đủ", "cây ổi",
                "cây hồng", "cây sen", "cây mai", "cây đào", "cây thông", "cây bàng",
                "cây phượng", "cây si", "cây đa", "cây gạo", "cây muỗm",
                "bàn ghế", "bàn học", "bàn làm", "ghế ngồi", "ghế đẩu", "giường ngủ",
                "cửa sổ", "cửa chính", "cửa phụ", "nhà cửa", "nhà bếp", "nhà trọ",
                "xe máy", "xe đạp", "xe buýt", "xe khách", "xe tải", "xe hơi",
                "đường phố", "đường làng", "đường hầm", "con đường", "ngõ nhỏ",
                "sông nước", "sông Hồng", "sông Cửu", "biển cả", "biển Đông",
                "núi non", "núi rừng", "đồi núi", "thung lũng", "cánh đồng",
                "mặt trời", "mặt trăng", "ngôi sao", "bầu trời", "mây trắng",
                "mưa to", "mưa nhỏ", "gió mạnh", "gió nhẹ", "nắng chang",
                "bữa sáng", "bữa trưa", "bữa tối", "bữa cơm", "món ăn",
                "cơm trắng", "cơm nước", "phở bò", "phở gà", "bún chả",
                "bún riêu", "bún bò", "cháo lòng", "cháo gà", "xôi gấc",
                "bánh mì", "bánh chưng", "bánh tét", "bánh cuốn", "bánh xèo",
                "trà đá", "trà sữa", "cà phê", "nước ngọt", "nước suối",
                "quả cam", "quả táo", "quả lê", "quả nho", "quả dưa",
                "rau muống", "rau cải", "rau ngót", "rau thơm", "rau xanh",
                "thịt bò", "thịt heo", "thịt gà", "thịt vịt", "thịt cá",
                "cá basa", "cá thu", "cá rô", "tôm hùm", "tôm sú",
                "cua biển", "ốc hương", "ốc bươu", "mực xào", "mực nướng",
                "học sinh", "học trò", "giáo viên", "thầy cô", "bạn bè",
                "bạn thân", "bạn học", "bạn hàng", "gia đình", "ông bà",
                "bố mẹ", "anh em", "chị gái", "em trai", "con trai",
                "con gái", "vợ chồng", "người yêu", "bạn gái", "bạn trai",
                "công việc", "công ty", "công nhân", "công an", "bác sĩ",
                "y tá", "kỹ sư", "luật sư", "nhà văn", "nhà thơ",
                "ca sĩ", "diễn viên", "đạo diễn", "nhạc sĩ", "họa sĩ",
                "trường học", "lớp học", "bài tập", "bài thi", "bài văn",
                "bài hát", "bài thơ", "cuốn sách", "quyển vở", "cây bút",
                "hộp bút", "cặp sách", "bảng đen", "phấn trắng", "bàn ghế",
                "máy tính", "điện thoại", "tivi led", "tủ lạnh", "máy giặt",
                "quạt máy", "điều hòa", "bóng đèn", "ổ cắm", "dây điện",
                "bức tranh", "bức ảnh", "tấm gương", "chiếc áo", "chiếc quần",
                "đôi giày", "đôi dép", "chiếc mũ", "chiếc nón", "chiếc túi",
                "con dao", "con thìa", "cái đũa", "cái bát", "cái đĩa",
                "cái nồi", "cái chảo", "cái thìa", "cái muỗng", "cái ấm",
                "lá cờ", "quốc kỳ", "quốc huy", "quốc ca", "dân tộc",
                "đất nước", "tổ quốc", "thành phố", "thị trấn", "thị xã",
                "huyện lỵ", "tỉnh lỵ", "thủ đô", "thủ phủ", "kinh đô",
                "làng xóm", "thôn quê", "xóm làng", "khu phố", "tổ dân",
                "chợ búa", "siêu thị", "cửa hàng", "quán ăn", "quán cà",
                "bệnh viện", "nhà thuốc", "tiệm tóc", "tiệm vàng", "ngân hàng",
                "bưu điện", "bưu cục", "sân bay", "ga tàu", "bến xe",
                "công viên", "vườn hoa", "vườn cây", "sân vườn", "hồ nước",
                "ao cá", "giếng nước", "suối nước", "thác nước", "đập nước",
                "làng nghề", "nghề nông", "nghề cá", "nghề thủ", "nghề mộc",
                "mùa xuân", "mùa hạ", "mùa thu", "mùa đông", "mùa màng",
                "năm mới", "năm cũ", "ngày mai", "ngày kia", "hôm nay",
                "hôm qua", "sáng sớm", "trưa nắng", "chiều tà", "đêm khuya",
                "buổi sáng", "buổi trưa", "buổi chiều", "buổi tối", "buổi đêm",
                "vui vẻ", "hạnh phúc", "buồn bã", "lo lắng", "tức giận",
                "yêu thương", "thương yêu", "kính trọng", "biết ơn", "tự hào",
                "chăm chỉ", "cần cù", "siêng năng", "thông minh", "thông minh",
                "hiền lành", "dịu dàng", "mạnh mẽ", "dũng cảm", "kiên trì",
                "trung thực", "thật thà", "ngay thẳng", "lịch sự", "lễ phép",
                "âm nhạc", "văn học", "mỹ thuật", "thể thao", "giải trí",
                "bóng đá", "cầu lông", "bóng chuyền", "bóng rổ", "bơi lội",
                "chạy bộ", "đi bộ", "leo núi", "câu cá", "đá bóng",
                "văn hóa", "lịch sử", "địa lý", "sinh học", "vật lý",
                "hóa học", "toán học", "tiếng Anh", "tiếng Việt", "ngoại ngữ",
                "màu đỏ", "màu xanh", "màu vàng", "màu trắng", "màu đen",
                "màu hồng", "màu tím", "màu cam", "màu nâu", "màu xám",
                "tay trái", "tay phải", "chân trái", "chân phải", "mắt trái",
                "mắt phải", "tai trái", "tai phải", "đầu gối", "khuỷu tay",
                "trái tim", "bộ não", "phổi thận", "gan mật", "dạ dày",
                "mèo con", "chó con", "gà con", "bò con", "cá chép",
                "cá rô", "chim sẻ", "chim cò", "hoa mai", "hoa đào",
                "hoa sen", "hoa hồng", "hoa cúc", "hoa lan", "hoa huệ",
                "tre xanh", "lá vàng", "lá xanh", "lá khô", "lá úa",
                "nước trong", "nước đục", "nước sâu", "nước cạn", "nước mặn",
                "gió bấc", "gió nam", "gió đông", "gió tây", "gió lùa",
                "mây đen", "mây trắng", "mây mưa", "sấm chớp", "sét đánh",
                "đất liền", "đất đai", "đất đỏ", "đất thịt", "đất cát",
                "vàng bạc", "sắt thép", "đồng đỏ", "nhôm kính", "gỗ tre",
                "giấy bút", "mực tím", "mực đen", "mực xanh", "mực đỏ",
                "tờ giấy", "tờ báo", "cuốn tạp", "tạp chí", "báo giấy",
                "tin tức", "thời sự", "thế giới", "trong nước", "quốc tế",
                "kinh tế", "chính trị", "xã hội", "văn hóa", "giáo dục",
                "y tế", "an ninh", "quốc phòng", "đối ngoại", "nội vụ",
                "luật pháp", "tòa án", "công lý", "công bằng", "tự do",
                "dân chủ", "hòa bình", "độc lập", "thống nhất", "phồn vinh",
                "giàu đẹp", "văn minh", "hiện đại", "truyền thống", "dân gian",
                "lễ hội", "tết Nguyên", "tết Trung", "tết Đoan", "tết Trung",
                "đèn lồng", "pháo hoa", "bánh kẹo", "mứt tết", "hoa quả",
                "lì xì", "áo dài", "khăn rằn", "nón lá", "guốc mộc",
                "đi guốc", "đi dép", "đi giày", "đi bộ", "đi xe",
                "lái xe", "chạy xe", "đỗ xe", "sửa xe", "rửa xe",
                "nấu cơm", "nấu ăn", "nấu canh", "nấu cháo", "nấu phở",
                "rửa bát", "rửa chén", "quét nhà", "lau nhà", "dọn dẹp",
                "giặt đồ", "phơi đồ", "ủi đồ", "gấp đồ", "treo đồ",
                "đọc sách", "đọc báo", "viết văn", "viết thơ", "viết chữ",
                "nghe nhạc", "hát karaoke", "xem phim", "xem tivi", "lướt web",
                "chơi game", "chơi bài", "chơi cờ", "chơi đàn", "chơi thể",
                "ngủ ngon", "ngủ sớm", "thức khuya", "thức dậy", "ngủ trưa",
                "ăn sáng", "ăn trưa", "ăn tối", "ăn vặt", "ăn chơi",
                "uống nước", "uống trà", "uống cà", "uống rượu", "uống bia",
                "tắm rửa", "tắm biển", "tắm nắng", "tắm gội", "gội đầu",
                "đánh răng", "rửa mặt", "soi gương", "chải tóc", "cắt tóc",
                "mua sắm", "bán hàng", "trả tiền", "thu tiền", "tiết kiệm",
                "tiêu tiền", "kiếm tiền", "mất tiền", "có tiền", "hết tiền",
                "làm việc", "nghỉ ngơi", "nghỉ phép", "nghỉ hè", "nghỉ lễ",
                "đi làm", "đi học", "đi chơi", "đi du", "đi thăm",
                "về nhà", "về quê", "ra phố", "vào phố", "lên phố",
                "xuống phố", "sang đường", "qua đường", "băng qua", "đi ngang",
                "đứng yên", "ngồi yên", "nằm yên", "chạy nhanh", "chạy chậm",
                "đi nhanh", "đi chậm", "nói to", "nói nhỏ", "nói chậm",
                "cười lớn", "cười nhỏ", "khóc to", "khóc thầm", "hét lớn",
                "thì thầm", "bí mật", "công khai", "rõ ràng", "mơ hồ",
                "dễ hiểu", "khó hiểu", "đơn giản", "phức tạp", "rắc rối",
                "thành công", "thất bại", "chiến thắng", "thua cuộc", "hòa nhau",
                "mèo hoang", "chó hoang", "gà rừng", "cá mập", "cá heo",
                "cá voi", "bò sữa", "sữa tươi", "sữa chua", "sữa đặc",
                "bánh ngọt", "bánh mặn", "kẹo ngọt", "kẹo chua", "kẹo dẻo",
                "hoa quả", "trái cây", "rau củ", "củ quả", "thực phẩm",
                "thức ăn", "đồ uống", "đồ ăn", "đồ chơi", "đồ dùng",
                "đồ đạc", "đồ cũ", "đồ mới", "đồ đẹp", "đồ xấu",
                "nhà đẹp", "nhà xấu", "nhà to", "nhà nhỏ", "nhà cao",
                "phòng ngủ", "phòng khách", "phòng tắm", "phòng học", "phòng ăn",
                "sân thượng", "sân vườn", "ban công", "hành lang", "cầu thang",
                "tầng một", "tầng hai", "tầng ba", "tầng bốn", "tầng năm",
                "lầu một", "lầu hai", "lầu ba", "lầu bốn", "lầu năm",
                "xe đạp", "xe máy", "ô tô", "tàu hỏa", "tàu thủy",
                "máy bay", "tên lửa", "vệ tinh", "trạm không", "bến cảng",
                "cảng biển", "cảng hàng", "sân bay", "ga hành", "vé máy",
                "hộ chiếu", "thị thực", "giấy tờ", "bằng cấp", "chứng chỉ",
                "học bổng", "học phí", "tiền học", "tiền nhà", "tiền ăn",
                "lương tháng", "lương năm", "thưởng tết", "phụ cấp", "bảo hiểm",
                "sức khỏe", "thể lực", "tinh thần", "tâm lý", "cảm xúc",
                "tình yêu", "tình bạn", "tình thân", "tình nghĩa", "tình cảm",
                "kỷ niệm", "kỷ luật", "kỷ cương", "nề nếp", "thói quen",
                "tính cách", "phẩm chất", "đạo đức", "nhân cách", "lương tâm",
                "trí tuệ", "trí nhớ", "trí tưởng", "trí tưởng", "trí khôn",
                "kinh nghiệm", "bài học", "câu chuyện", "câu hỏi", "câu trả",
                "câu thơ", "câu văn", "câu nói", "câu chuyện", "câu chuyện",
                "mảnh ghép", "mảnh vỡ", "mảnh đời", "mảnh đất", "mảnh vườn",
                "vùng đất", "vùng biển", "vùng núi", "vùng trời", "vùng quê",
                "miền Bắc", "miền Nam", "miền Trung", "miền Tây", "miền Đông",
                "Tây Nguyên", "đồng bằng", "cao nguyên", "bán đảo", "quần đảo",
                "đảo lớn", "đảo nhỏ", "hòn đảo", "bãi biển", "bãi cát",
                "cát trắng", "cát vàng", "đá cuội", "đá to", "đá nhỏ",
                "hòn đá", "hòn non", "ngọn núi", "đỉnh núi", "chân núi",
                "sườn núi", "khe núi", "hang động", "động đá", "suối nguồn",
                "nguồn nước", "nguồn sống", "nguồn gốc", "nguồn cơn", "nguồn lực",
                "sức mạnh", "sức khỏe", "sức sống", "sức ép", "sức kéo",
                "lực lượng", "lực học", "lực đẩy", "lực hút", "lực ma",
                "tốc độ", "gia tốc", "vận tốc", "quãng đường", "quãng thời",
                "thời gian", "không gian", "vũ trụ", "thiên hà", "ngôi sao",
                "hành tinh", "vệ tinh", "mặt trăng", "mặt trời", "ánh sáng",
                "bóng tối", "bóng đêm", "bóng râm", "bóng mát", "bóng đổ",
                "tiếng nói", "tiếng hát", "tiếng cười", "tiếng khóc", "tiếng reo",
                "tiếng gọi", "tiếng gõ", "tiếng đập", "tiếng chuông", "tiếng trống",
                "mùi thơm", "mùi hôi", "mùi hăng", "mùi chua", "mùi ngọt",
                "vị mặn", "vị chua", "vị ngọt", "vị đắng", "vị cay",
                "cảm giác", "cảm nhận", "cảm thông", "cảm ơn", "cảm kích",
                "tình nguyện", "tình người", "tình làng", "tình xóm", "tình nghĩa",
                "nghĩa tình", "nghĩa cử", "nghĩa vụ", "nghĩa vụ", "nghĩa vụ",
                "trách nhiệm", "bổn phận", "nghĩa vụ", "quyền lợi", "quyền hạn",
                "tự do", "bình đẳng", "bác ái", "nhân ái", "nhân hậu",
                "khoan dung", "tha thứ", "buông bỏ", "gác lại", "bỏ qua",
                "bỏ lỡ", "bỏ phí", "lãng phí", "tiết kiệm", "dành dụm",
                "tích lũy", "tích trữ", "dự trữ", "chuẩn bị", "sẵn sàng",
                "chu đáo", "tỉ mỉ", "cẩn thận", "cẩn trọng", "thận trọng",
                "mạo hiểm", "liều lĩnh", "táo bạo", "quả cảm", "dũng cảm",
                "nhút nhát", "rụt rè", "ngại ngùng", "xấu hổ", "tự ti",
                "tự tin", "tự trọng", "tự tôn", "tự hào", "tự kiêu",
                "khiêm tốn", "nhún nhường", "nhường nhịn", "nhịn nhục", "cam chịu",
                "chịu đựng", "gánh vác", "gánh nặng", "gánh chịu", "gánh họa",
                "mang lại", "mang đi", "mang theo", "mang về", "mang đến",
                "đem lại", "đem đến", "đem về", "đem đi", "đem theo",
                "gửi thư", "gửi tin", "gửi hàng", "gửi tiền", "gửi quà",
                "nhận thư", "nhận tin", "nhận hàng", "nhận tiền", "nhận quà",
                "trao đổi", "trao tặng", "trao phần", "trao giải", "trao huy",
                "vinh danh", "vinh quang", "vinh dự", "vinh hiển", "vinh quý",
                "danh dự", "danh tiếng", "danh vọng", "danh lam", "danh thắng",
                "kỳ quan", "kỳ tích", "kỳ diệu", "kỳ lạ", "kỳ thú",
                "thú vị", "hấp dẫn", "lôi cuốn", "cuốn hút", "say mê",
                "đam mê", "nhiệt huyết", "nhiệt tình", "hăng hái", "hăng say",
                "lười biếng", "lười nhác", "lười biếng", "lười biếng", "lười biếng",
                "mèo mun", "chó vàng", "gà trống", "gà mái", "vịt trời",
                "bò vàng", "trâu xanh", "ngựa đen", "ngựa trắng", "lợn đen",
                "cá vàng", "cá bạc", "tôm càng", "cua đồng", "ốc nhồi",
                "tre già", "tre non", "tre xanh", "lá tre", "măng tre",
                "gạo trắng", "gạo nếp", "gạo tẻ", "gạo thơm", "gạo dẻo",
                "bột gạo", "bột mì", "bột năng", "bột sắn", "bột ngô",
                "dầu ăn", "dầu oliu", "dầu mè", "dầu cá", "dầu thực",
                "muối ăn", "muối biển", "muối iốt", "đường trắng", "đường đen",
                "nước mắm", "nước tương", "tương ớt", "tương cà", "mắm tôm",
                "mắm ruốc", "mắm nêm", "mắm tép", "mắm cá", "mắm tôm",
                "rau sống", "rau luộc", "rau xào", "rau nấu", "rau nướng",
                "thịt nướng", "thịt luộc", "thịt xào", "thịt kho", "thịt rang",
                "cá kho", "cá chiên", "cá hấp", "cá nướng", "cá luộc",
                "canh chua", "canh cải", "canh bí", "canh rau", "canh khổ",
                "chè đậu", "chè bưởi", "chè sen", "chè thái", "chè khúc",
                "kem dừa", "kem sữa", "kem que", "kem ly", "kem ốc",
                "sinh tố", "nước ép", "nước cam", "nước chanh", "nước dừa",
                "trà xanh", "trà đen", "trà olong", "trà nhài", "trà gừng",
                "bánh flan", "bánh bông", "bánh bò", "bánh da", "bánh pía",
                "mì gói", "mì tôm", "mì ý", "mì quảng", "mì xào",
                "cơm tấm", "cơm rang", "cơm chiên", "cơm hộp", "cơm văn",
                "xôi vò", "xôi đậu", "xôi nếp", "xôi sắn", "xôi bắp",
                "nem rán", "nem nướng", "nem chua", "chả giò", "chả lụa",
                "giò lụa", "giò thủ", "giò bì", "giò heo", "giò bò",
                "lạp xưởng", "xúc xích", "thịt nguội", "pate gan", "ruốc tôm",
                "mắm ruốc", "dưa chua", "dưa cải", "dưa leo", "dưa hấu",
                "dưa gang", "dưa lưới", "dưa lê", "dưa bở", "dưa vàng",
                "hoa cải", "hoa cúc", "hoa mai", "hoa đào", "hoa giấy",
                "lá chuối", "lá dong", "lá sen", "lá trầu", "lá cọ",
                "gốc cây", "thân cây", "cành cây", "ngọn cây", "rễ cây",
                "quả chín", "quả xanh", "quả vàng", "quả đỏ", "quả tím",
                "hạt giống", "hạt lúa", "hạt ngô", "hạt đậu", "hạt sen",
                "đồng ruộng", "ruộng lúa", "ruộng ngô", "ruộng đậu", "ruộng rau",
                "vườn rau", "vườn cây", "vườn ươm", "vườn tược", "vườn nhãn",
                "ao bèo", "ao sen", "ao cá", "ao tôm", "ao nuôi",
                "chuồng gà", "chuồng bò", "chuồng heo", "chuồng trâu", "chuồng ngựa",
                "hàng rào", "cổng làng", "cổng trường", "cổng chào", "cổng vào",
                "tường thành", "thành quách", "thành lũy", "thành trì", "thành phố",
                "pháo đài", "lâu đài", "cung điện", "đền thờ", "chùa chiền",
                "nhà thờ", "thánh đường", "đình làng", "đền Hùng", "lăng Bác",
                "tượng đài", "tượng Bác", "tượng đồng", "tượng đá", "tượng gỗ",
                "bức tường", "bức tranh", "bức ảnh", "bức thư", "bức thư",
                "lá thư", "lá đơn", "lá cờ", "lá bài", "lá số",
                "con số", "con đường", "con sông", "con kênh", "con lạch",
                "con kênh", "con đê", "con đập", "con phà", "con cầu",
                "cây cầu", "cầu tre", "cầu đá", "cầu sắt", "cầu bê",
                "đường ray", "đường sắt", "đường cao", "đường hầm", "đường tránh",
                "hầm chui", "hầm đi", "hầm xe", "hầm đường", "hầm núi",
                "cao tốc", "quốc lộ", "tỉnh lộ", "huyện lộ", "đường làng",
                "ngõ xóm", "hẻm nhỏ", "hẻm phố", "phố cổ", "phố mới",
                "khu phố", "khu đô", "khu công", "khu dân", "khu tập",
                "chung cư", "nhà tập", "nhà trọ", "nhà thuê", "nhà riêng",
                "biệt thự", "nhà vườn", "nhà sàn", "nhà lá", "nhà tranh",
                "mái nhà", "mái hiên", "mái che", "mái vòm", "mái ngói",
                "ngói âm", "ngói dương", "ngói đỏ", "ngói xanh", "ngói men",
                "gạch đỏ", "gạch men", "gạch ốp", "gạch lát", "gạch block",
                "xi măng", "bê tông", "cát xây", "đá xây", "sắt thép",
                "cột điện", "dây điện", "trạm điện", "nhà máy", "thủy điện",
                "nhiệt điện", "điện gió", "điện mặt", "năng lượng", "tiết kiệm",
                "ô nhiễm", "bảo vệ", "môi trường", "khí hậu", "thời tiết",
                "thủy văn", "địa chất", "địa hình", "bản đồ", "la bàn",
                "kinh tuyến", "vĩ tuyến", "xích đạo", "cực Bắc", "cực Nam",
                "bán cầu", "toàn cầu", "thế giới", "châu Á", "châu Âu",
                "châu Mỹ", "châu Phi", "châu Đại", "châu Nam", "Đông Nam",
                "Việt Nam", "Hà Nội", "Sài Gòn", "Đà Nẵng", "Huế xưa",
                "Hội An", "Nha Trang", "Vũng Tàu", "Phú Quốc", "Hạ Long",
                "Sapa xanh", "Đà Lạt", "Pleiku đỏ", "Buôn Ma", "Cần Thơ",
                "mèo tam", "chó ta", "gà ta", "vịt xiêm", "ngỗng trắng",
                "cá thu", "cá ngừ", "cá hồi", "cá tra", "cá lóc",
                "tôm hùm", "tôm càng", "tôm sú", "tôm thẻ", "tôm khô",
                "cua đồng", "cua biển", "ghẹ xanh", "ghẹ đỏ", "sò điệp",
                "nghêu sò", "hến xào", "ốc hương", "ốc mít", "ốc len",
                "mực ống", "mực nang", "bạch tuộc", "sa tôm", "cua rang",
                "gà luộc", "gà rán", "gà quay", "gà nướng", "gà hấp",
                "vịt quay", "vịt luộc", "vịt nướng", "ngỗng quay", "ngỗng luộc",
                "heo quay", "heo luộc", "heo kho", "heo nướng", "heo rang",
                "bò bít", "bò kho", "bò nướng", "bò xào", "bò luộc",
                "dê nướng", "dê luộc", "dê hấp", "dê xào", "dê kho",
                "cháo lòng", "cháo gà", "cháo vịt", "cháo ếch", "cháo cá",
                "lẩu thái", "lẩu bò", "lẩu cá", "lẩu gà", "lẩu dê",
                "gỏi cuốn", "gỏi đu", "gỏi ngó", "gỏi xoài", "gỏi bắp",
                "nộm đu", "nộm gà", "nộm bò", "nộm tôm", "nộm cua",
                "bún bò", "bún riêu", "bún chả", "bún thịt", "bún cá",
                "miến gà", "miến lươn", "miến ngan", "miến vịt", "miến cua",
                "hủ tiếu", "bánh canh", "bánh đa", "bánh đúc", "bánh tằm",
                "cao lầu", "mì quảng", "bánh hỏi", "bánh ướt", "bánh cuốn",
                "xôi gấc", "xôi vò", "xôi đậu", "xôi sắn", "xôi ngô",
                "chè đậu", "chè bưởi", "chè sen", "chè khoai", "chè bắp",
                "bánh flan", "bánh su", "bánh bông", "bánh pía", "bánh da",
                "kem dừa", "kem sữa", "kem que", "kem ốc", "kem ly",
                "nước mía", "nước dừa", "nước chanh", "nước cam", "nước ép",
                "trà đá", "trà sữa", "trà chanh", "trà tắc", "trà gừng",
                "cà phê", "cà phin", "cà phê", "cà sữa", "cà đá",
                "bia hơi", "bia chai", "bia lon", "rượu nếp", "rượu gạo",
                "rượu táo", "rượu nho", "rượu mạnh", "rượu ngọt", "rượu chua",
                "mèo nhà", "chó nhà", "gà nhà", "vịt nhà", "bò nhà",
                "tre nhà", "cây nhà", "vườn nhà", "sân nhà", "bếp nhà",
                "phòng khách", "phòng ngủ", "phòng bếp", "phòng tắm", "phòng học",
                "bàn học", "bàn ăn", "bàn làm", "bàn uống", "bàn chơi",
                "ghế sofa", "ghế bành", "ghế gỗ", "ghế nhựa", "ghế xếp",
                "giường ngủ", "giường tầng", "giường gỗ", "giường sắt", "giường bệt",
                "tủ quần", "tủ áo", "tủ sách", "tủ giày", "tủ lạnh",
                "kệ sách", "kệ giày", "kệ tivi", "kệ bếp", "kệ đựng",
                "thảm trải", "thảm len", "thảm nhựa", "thảm cỏ", "thảm lông",
                "rèm cửa", "rèm vải", "rèm tre", "rèm nhựa", "rèm gỗ",
                "gương soi", "gương treo", "gương đứng", "gương tròn", "gương vuông",
                "đồng hồ", "đồng tre", "đồng đỏ", "đồng thau", "đồng nhôm",
                "bình hoa", "bình nước", "bình trà", "bình pha", "bình giữ",
                "lọ hoa", "lọ thủy", "lọ sứ", "lọ gốm", "lọ đất",
                "chậu cây", "chậu hoa", "chậu sứ", "chậu nhựa", "chậu xi",
                "dụng cụ"
            ];
        }
    }
}
