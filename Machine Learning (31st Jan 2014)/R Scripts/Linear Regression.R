data(airquality)
?airquality
names(airquality)
head(airquality)

#[1] "Ozone"   "Solar.R" "Wind"    "Temp"    "Month"   "Day"  
#Ozone = Response variable
#Solar.R = Explainatory Variable
#Wind = Explainatory Variable

plot(Ozone~Solar.R,data=airquality)

mean.Ozone = mean(airquality$Ozone,na.rm=T)

abline(h=mean.Ozone)

#use lm to fit a regression line through these data :

model1= lm(Ozone~Solar.R, data=airquality)

model1

predict(model1)

predict(model1,data.frame(Solar.R=100))

abline(model1, col="red") 

plot(model1)

summary(model1)

#mutiple regression considering Wind and Solar.R

coplot(Ozone~Solar.R|Wind,panel=panel.smooth,airquality)

model2 = lm(Ozone~Solar.R*Wind,airquality)

plot(model2)