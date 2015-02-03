Iris = read.csv(file.choose(),header=TRUE)
head(Iris)
Iris.data <- Iris
Iris.data$class <-NULL

View(Iris.data)

results = kmeans(Iris.data,3)

View(results)

table(Iris$class,results$cluster)

plot(Iris[c("petal.length","petal.width")],col=results$cluster)

plot(Iris[c("petal.length","petal.width")],col=Iris$class)

plot(Iris[c("sepal.length","sepal.width")],col=Iris$class)

plot(Iris[c("sepal.length","sepal.width")],col=results$cluster)