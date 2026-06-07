import pandas as pd
from sklearn.preprocessing import StandardScaler
from sklearn.cluster import KMeans
import matplotlib.pyplot as plt
# read data
original_df = pd.read_csv("netflix_titles.csv")

print("===== THÔNG TIN DATASET =====")
print("Shape:", original_df.shape)
print("Thông tin:")
print(original_df.info())

print("\n===== DỮ LIỆU THIẾU =====")
print(original_df.isnull().sum())

print(original_df["type"].value_counts())
print(original_df["rating"].value_counts())
print(original_df["duration"].value_counts())
print(original_df["release_year"].value_counts())
print(original_df["listed_in"].value_counts().head(10))
print(original_df["country"].value_counts().head(10))

#clone 
df = original_df.copy()

# drop unnecessary columns
df = df.drop(columns=
             [
              "director",
              "cast",
              "date_added", 
              "description"])

# rows_to_drop = df[df['rating'].isna() | df['duration'].isna()]
# print(f"\n===== TỔNG SỐ DÒNG SẼ BỊ XÓA: {len(rows_to_drop)} DÒNG =====")
# # In ra các cột quan trọng để bạn dễ nhìn và đối chiếu
# print(rows_to_drop[['show_id', 'title', 'type', 'rating', 'duration']])

#fill missing values with Unknown
df['country'] = df['country'].fillna('Unknown')

#drop 7 rows from rating and duration
df = df.dropna(subset=["rating", "duration"])

# divide movie and tv show data
movie_df = df[df['type'] == 'Movie'].copy()
tvshow_df = df[df['type'] == 'TV Show'].copy()

print("Movie:", movie_df.shape)
print("TV Show:", tvshow_df.shape)



movie_df["duration_num"]= movie_df['duration'].str.extract('(\d+)').astype(float)
rating_encoded = pd.get_dummies(movie_df['rating'], prefix='rating',dtype=int)
genre_encoded = movie_df['listed_in'].str.get_dummies(sep=', ')
country_encoded = movie_df['country'].str.get_dummies(sep=', ')

numeric_features = movie_df[['release_year', 'duration_num']]

scaler = StandardScaler()
numeric_scaled = pd.DataFrame(
    scaler.fit_transform(numeric_features),
    columns=numeric_features.columns,
    index=movie_df.index
)

X_movie = pd.concat([ numeric_scaled, rating_encoded, genre_encoded, country_encoded ], axis=1)

print("===== SAU KHI SCALE =====")
print(X_movie.head())

wcss = []
k_range = range(1, 30) # Chạy thử từ 1 đến 30 cụm

print("===== ĐANG TÍNH TOÁN ELBOW METHOD (VUI LÒNG ĐỢI) =====")
for k in k_range:
    kmeans = KMeans(n_clusters=k, init='k-means++', random_state=42)
    kmeans.fit(X_movie)
    wcss.append(kmeans.inertia_) # inertia_ chính là WCSS
print("-> Tính toán xong!")

# ==========================================
# 2. VẼ BIỂU ĐỒ ELBOW
# ==========================================
plt.figure(figsize=(10, 6))
plt.plot(k_range, wcss, marker='o', linestyle='--', color='b')
plt.title('Phương pháp Khuỷu tay (Elbow Method) cho dữ liệu Netflix')
plt.xlabel('Số lượng cụm (K)')
plt.ylabel('WCSS (Inertia)')
plt.xticks(k_range)
plt.grid(True)

# Hiển thị biểu đồ lên màn hình
plt.show()



# kmean
num_clusters = 20
kmeans = KMeans(n_clusters=num_clusters, random_state=42)
cluster_labels = kmeans.fit_predict(X_movie)
movie_df['cluster'] = cluster_labels
output_file = "netflix_movies_clustered.csv"
movie_df.to_csv(output_file, index=False, encoding='utf-8-sig')
print(f"\n===== ĐÃ TRAIN VÀ XUẤT FILE '{output_file}' THÀNH CÔNG =====")
print("Số lượng phim trong mỗi cụm:")
print(movie_df['cluster'].value_counts().sort_index())

# # Thử in ra 3 phim tiêu biểu của mỗi cụm để xem thử
# print("\n===== GỢI Ý XEM THỬ PHIM TRONG CÁC CỤM =====")
# for i in range(num_clusters):
#     print(f"\nCụm {i}:")
#     print(movie_df[movie_df['cluster'] == i][['title', 'listed_in', 'release_year']].head(3))
