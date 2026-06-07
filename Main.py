import pandas as pd
import matplotlib.pyplot as plt
from sklearn.preprocessing import StandardScaler
from sklearn.cluster import KMeans

class ContentCluster :
    def __init__(self,filePath= "netflix_titles.csv"):
        self._filePath= filePath
        self._rawDf= None
        self._movieDf= None
        self._tvShowDf= None

    def LoadAndProcessData(self):
        print("Loading and processing data...")
        self._rawDf = pd.read_csv(self._filePath)
        df = self._rawDf.copy()

        columnsToDrop= [
            "director",
            "cast",
            "date_added",
            "description"
        ]
        df = df.drop(columns=columnsToDrop)

        df['country'] = df['country'].fillna('Unknown')
        df = df.dropna(subset=["rating", "duration"])

        self._movieDf = df[df['type'] == 'Movie'].copy()
        self._tvShowDf = df[df['type'] == 'TV Show'].copy()
        print(f"Tổng số lượng nội dung -> Movie: {self._movieDf.shape[0]} phim | TV Show: {self._tvShowDf.shape[0]} bộ")
        print("Data loading and processing complete.")

    def PrepareFeature(self,df):
        print("Preparing features...")
        dfProcessed = df.copy()
        dfProcessed['duration_num'] = dfProcessed['duration'].str.extract('(\d+)').astype(float)

        ratingEncoded = pd.get_dummies(dfProcessed['rating'], prefix='rating',dtype=int)
        genresEncoded= dfProcessed['listed_in'].str.get_dummies(sep=', ')
        countryEncoded = dfProcessed['country'].str.get_dummies(sep=', ')

        numericFeatures= dfProcessed[['release_year', 'duration_num']]
        scaler = StandardScaler()
        numericScaled=pd.DataFrame(
            scaler.fit_transform(numericFeatures),
            columns=numericFeatures.columns,
            index=dfProcessed.index
        )

        X=pd.concat([numericScaled, ratingEncoded, genresEncoded, countryEncoded], axis=1)
        return X

    def ElbowMethod(self, contentType= "Movie",k=20):
        if contentType == "Movie":
            df = self._movieDf
        else:
            df = self._tvShowDf

        X = self.PrepareFeature(df)
        inertia = []
        for i in range(1, k+1):
            kmeans = KMeans(n_clusters=i,init='k-means++',random_state=42,n_init=10)
            kmeans.fit(X)
            inertia.append(kmeans.inertia_)

        plt.figure(figsize=(10, 5))
        plt.plot(range(1, k+1), inertia, marker='o')
        plt.title(f"Elbow Method for {contentType}s")
        plt.xlabel("Number of Clusters")
        plt.ylabel("Inertia")
        plt.show()
    
    def ClusterAndSave(self,contentType="Movie",nClusters= 20,outputFile=None):
        if contentType == "Movie":
            df = self._movieDf
        else:
            df = self._tvShowDf

        X = self.PrepareFeature(df)
        kmeans = KMeans(n_clusters=nClusters, init='k-means++', random_state=42, n_init=10)
        df['cluster'] = kmeans.fit_predict(X)

        if outputFile:
            df.to_csv(outputFile, index=False,encoding='utf-8-sig')
            print(f"Clustered data saved to {outputFile}")


if __name__ == "__main__":
    content_cluster = ContentCluster(filePath="netflix_titles.csv")
    content_cluster.LoadAndProcessData()
    #content_cluster.ElbowMethod(contentType="Movie", k=50)
    #content_cluster.ClusterAndSave(contentType="Movie", nClusters=20, outputFile="clustered_movies.csv")
    #content_cluster.ElbowMethod(contentType="TV Show", k=50)
    #content_cluster.ClusterAndSave(contentType="TV Show", nClusters=20, outputFile="clustered_tv_shows.csv")
