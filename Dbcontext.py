import pandas as pd
import pyodbc

# 1. Đọc dữ liệu từ file CSV
csv_file = 'clustered_content.csv'
df = pd.read_csv(csv_file)

# 2. Cấu hình chuỗi kết nối tới SQL Server của bạn
# Hãy thay đổi các thông số SERVER, DATABASE, UID, PWD cho đúng cấu hình hệ thống của bạn
conn_str = (
    r"DRIVER={ODBC Driver 17 for SQL Server};"
    r"SERVER=HAI2\SQLEXPRESS;"        # Tên Server của bạn (VD: localhost hoặc .\SQLEXPRESS)
    r"DATABASE=NetflixDb;"    # Tên Database vừa tạo cấu hình ở trên
    #r"UID=YOUR_USERNAME;"              # Tài khoản đăng nhập SQL Server (nếu có)
    #r"PWD=YOUR_PASSWORD;"              # Mật khẩu đăng nhập SQL Server (nếu có)
    # LƯU Ý: Nếu bạn sử dụng Windows Authentication (không cần nhập UID/PWD), hãy bỏ dấu thăng (#) ở dòng dưới:
    r"Trusted_Connection=yes;"
)

try:
    # Thiết lập kết nối
    conn = pyodbc.connect(conn_str)
    cursor = conn.cursor()
    print("Kết nối thành công tới SQL Server!")
    
    # KÍCH HOẠT TÍNH NĂNG TĂNG TỐC: Giúp truyền dữ liệu mảng lớn cực kì nhanh (Bulk Insert)
    cursor.fast_executemany = True

    # -------------------------------------------------------------
    # BƯỚC 1: Chèn dữ liệu vào bảng 'contents'
    # -------------------------------------------------------------
    print("Đang tiến hành chèn dữ liệu vào bảng contents...")
    insert_content_sql = """
    INSERT INTO contents (show_id, title, type, cluster_id) 
    VALUES (?, ?, ?, ?)
    """
    # Lấy các cột tương ứng từ dataframe và chuyển thành list.
    # Cột 'cluster' trong file CSV sẽ được ánh xạ vào trường 'cluster_id' trong DB.
    contents_data = df[['show_id', 'title', 'type', 'cluster']].values.tolist()
    
    cursor.executemany(insert_content_sql, contents_data)
    conn.commit()
    print(f"-> Đã chèn thành công {len(contents_data)} bộ phim vào bảng contents.")

    # -------------------------------------------------------------
    # BƯỚC 2: Tách từ chuỗi và chèn dữ liệu vào bảng danh mục 'genres'
    # -------------------------------------------------------------
    print("Đang xử lý danh sách thể loại phim (genres)...")
    
    # Cột 'listed_in' chứa các thể loại ngăn cách bằng dấu phẩy, dùng set để lấy danh sách duy nhất
    unique_genres = set()
    for genres_str in df['listed_in'].dropna():
        genres_list = [g.strip() for g in genres_str.split(',')]
        unique_genres.update(genres_list)
    
    # Kiểm tra xem những thể loại này đã tồn tại trong DB chưa (tránh lỗi trùng lặp dữ liệu)
    cursor.execute("SELECT genre_name FROM genres")
    existing_genres = set(row[0] for row in cursor.fetchall())
    
    # Tìm các thể loại mới hoàn toàn
    new_genres = unique_genres - existing_genres
    
    if new_genres:
        insert_genre_sql = "INSERT INTO genres (genre_name) VALUES (?)"
        genre_data = [(g,) for g in new_genres]
        cursor.executemany(insert_genre_sql, genre_data)
        conn.commit()
        print(f"-> Đã chèn thêm {len(new_genres)} thể loại mới vào bảng genres.")
    
    # Lấy lại bảng ánh xạ hoàn chỉnh từ database { tên_thể_loại : mã_id_tự_tăng }
    cursor.execute("SELECT genre_id, genre_name FROM genres")
    genre_mapping = {row.genre_name: row.genre_id for row in cursor.fetchall()}

    # -------------------------------------------------------------
    # BƯỚC 3: Tạo và chèn dữ liệu vào bảng liên kết nhiều-nhiều 'content_genre'
    # -------------------------------------------------------------
    print("Đang tạo liên kết phim và thể loại (content_genre)...")
    content_genre_data = []
    
    for _, row in df.iterrows():
        show_id = row['show_id']
        genres_str = row['listed_in']
        if pd.notna(genres_str):
            genres_list = [g.strip() for g in genres_str.split(',')]
            for g_name in genres_list:
                if g_name in genre_mapping:
                    g_id = genre_mapping[g_name]
                    content_genre_data.append((show_id, g_id))
                    
    insert_content_genre_sql = "INSERT INTO content_genre (show_id, genre_id) VALUES (?, ?)"
    cursor.executemany(insert_content_genre_sql, content_genre_data)
    conn.commit()
    print(f"-> Đã chèn thành công {len(content_genre_data)} liên kết vào bảng content_genre.")
    
    print("\n[THÀNH CÔNG] DỮ LIỆU ĐÃ ĐƯỢC LƯU HOÀN TOÀN VÀO SQL SERVER!")

except Exception as e:
    print("\n[THẤT BẠI] Có lỗi xảy ra trong quá trình nạp dữ liệu:")
    print(e)
    if 'conn' in locals():
        conn.rollback()
        print("Đã thực hiện Rollback hệ thống để tránh lỗi phân mảnh dữ liệu.")
        
finally:
    if 'conn' in locals():
        cursor.close()
        conn.close()
        print("Đã đóng kết nối CSDL an toàn.")