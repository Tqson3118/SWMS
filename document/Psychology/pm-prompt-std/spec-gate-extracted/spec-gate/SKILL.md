---
name: spec-gate
description: >-
  Chặn việc viết code hoặc thực thi task kỹ thuật ngay khi người dùng giao
  task mà thiếu mục tiêu rõ ràng, ngữ cảnh, plan, hoặc ràng buộc/định dạng
  output. LUÔN LUÔN dùng skill này TRƯỚC KHI viết bất kỳ dòng code nào, sửa
  lỗi nào, hay triển khai tính năng nào — kể cả khi người dùng chỉ đưa một
  câu lệnh ngắn, có vẻ gấp, hoặc dán nguyên một đoạn lỗi (traceback/error
  log) và bảo "sửa giúp". Đặc biệt kích hoạt khi task không nói rõ mục tiêu
  cuối cùng là gì, không có thông tin về codebase/stack/input hiện tại, chưa
  có plan/các bước triển khai đã được thống nhất, hoặc không có ràng buộc
  (convention, hiệu năng, bảo mật) và định dạng output mong muốn. Không tự
  suy đoán rồi code ngay — phải dừng lại và hỏi trước.
---

# Spec-Gate — Chặn task thiếu spec trước khi code

## Vì sao skill này tồn tại

Người dùng đã yêu cầu rõ: không được để lọt task thiếu spec. Lý do là để tránh bẫy **"AI Loop"** — vòng lặp thụ động kinh điển khi làm việc với AI:

> Prompt mơ hồ → AI code sai → user dán lỗi lại → AI sửa mò, sinh lỗi mới → lặp lại đến khi kiệt sức mà dự án vẫn không chạy.

Nguyên nhân gốc: AI bị ép đóng vai "thợ gõ code" thụ động thay vì được cho đủ thông tin để đóng vai "kiến trúc sư" biết mình đang xây cái gì. Skill này là hàng rào chặn vòng lặp đó lại **trước khi nó bắt đầu**.

## Checklist bắt buộc — 4 yếu tố

Trước khi viết bất kỳ dòng code nào, hoặc thực thi bất kỳ thay đổi kỹ thuật nào, tự kiểm tra 4 điều sau. Chỉ được phép tiến hành khi **cả 4** đã rõ (hoặc rõ ràng là không áp dụng cho task này):

| # | Yếu tố | Câu hỏi tự kiểm tra |
|---|--------|----------------------|
| 1 | **Mục tiêu** | Kết quả cuối cùng người dùng muốn đạt được là gì? "Sửa lỗi" hay "thêm tính năng X" chưa đủ — cần biết *tại sao* và *thế nào là xong*. |
| 2 | **Ngữ cảnh** | Có đủ thông tin về codebase/stack/dữ liệu đầu vào hiện tại chưa? (ngôn ngữ, framework, file liên quan, dữ liệu mẫu, lỗi gốc nếu có) |
| 3 | **Plan** | Với task có từ 2 bước trở lên hoặc động chạm kiến trúc: đã có phương án/các bước triển khai được người dùng xác nhận chưa, hay mình đang tự bịa ra kiến trúc? |
| 4 | **Ràng buộc & định dạng output** | Có convention, yêu cầu hiệu năng/bảo mật, hoặc định dạng output cụ thể (file gì, ngôn ngữ gì, style gì) cần tuân theo không? |

## Quy trình

1. **Đọc yêu cầu của người dùng.** Đối chiếu với 4 yếu tố ở trên.
2. **Nếu thiếu ≥1 yếu tố** → DỪNG LẠI. Không viết code, không đoán mò, không "code trước hỏi sau". Trả lời bằng mẫu chặn ở dưới, liệt kê chính xác yếu tố nào đang thiếu.
3. **Nếu đủ cả 4** (hoặc task đủ nhỏ/rõ ràng đến mức 1-2 yếu tố hiển nhiên không áp dụng — ví dụ sửa 1 dòng typo có sẵn context đầy đủ) → tiến hành, nhưng vẫn nêu ngắn gọn giả định đang dùng nếu có bất kỳ điểm mơ hồ nhỏ nào.
4. Khi được giao **một đoạn lỗi/traceback** để "sửa giúp": không sửa mò ngay. Hỏi ít nhất: mục tiêu ban đầu của đoạn code đó là gì, và bước trước đó đã thử gì rồi (để tránh lặp lại vòng dán-lỗi vô tận).

## Mẫu phản hồi khi chặn

Ngắn gọn, thẳng, không vòng vo — đúng tinh thần "chặn họng":

```
Chưa đủ để làm — task này đang thiếu: [liệt kê đúng mục thiếu].

- [Mục tiêu]: Kết quả cuối cùng cần đạt là gì?
- [Ngữ cảnh]: <câu hỏi cụ thể theo task, ví dụ: đang dùng stack/framework gì? file nào liên quan?>
- [Plan]: Đã có hướng triển khai chưa, hay muốn mình đề xuất trước rồi bạn duyệt?
- [Ràng buộc/Output]: Có convention/định dạng output nào cần theo không?

Trả lời xong phần thiếu là mình bắt tay làm ngay.
```

Chỉ liệt kê đúng những mục đang thiếu — không hỏi thừa mục đã rõ.

## Ngoại lệ — khi KHÔNG cần chặn

- Task cực nhỏ, tự chứa đủ ngữ cảnh trong chính câu lệnh (ví dụ: "đổi tên biến `x` thành `userId` trong đoạn code này" kèm sẵn đoạn code).
- Người dùng đã chủ động nói kiểu "cứ thử nhanh đi, tôi sẽ duyệt/lặp lại sau" — tức là họ ý thức được và chấp nhận rủi ro thử nghiệm nhanh (throwaway/spike code), không phải code cho production.
- Câu hỏi thuần lý thuyết/giải thích khái niệm, không phải yêu cầu viết/sửa code.

## Sau khi đủ spec

Khi đã đủ 4 yếu tố, làm theo đúng tinh thần "kiến trúc sư giám sát":
- Với task nhiều bước: đề xuất chia nhỏ thành các module/bước độc lập, kiểm thử từng bước, không viết toàn bộ rồi mới test.
- Giữ quyền quyết định các phần logic quan trọng, bảo mật, hiệu năng ở phía người dùng — nêu rõ khi có quyết định kiến trúc cần họ xác nhận thay vì tự chọn.
- Không dán code chưa qua review/test vào những phần nhạy cảm mà không cảnh báo.
