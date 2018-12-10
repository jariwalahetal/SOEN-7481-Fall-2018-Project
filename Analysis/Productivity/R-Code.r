# install.packages("MASS")
library(MASS)
# install.packages("pscl")
library(pscl)
# insall.packages("AER")
library(AER)

mydata<- read.csv("C:\\Users\\win10\\Desktop\\productivity.csv")
attach(mydata)
	
# Define variables
Y1 <- cbind(merged_pr_by_core_developer)
Y2 <- cbind(rejected_pr_by_core_developers)
Y3 <- cbind(merged_pr_by_ext_developer)
Y4 <- cbind(rejected_pr_by_ext_developer)

X1 <- cbind(ï..team_size, proj_age, computed_n_forks, computed_n_src_loc, ci_integration)
X2 <- cbind(ï..team_size, proj_age, computed_n_forks, computed_n_src_loc, ci_integration)

# Descriptive statistics
summary(Y1)
summary(Y2)
summary(Y3)
summary(Y4)
summary(X1)
summary(X2)

# Poisson model coefficients
poissonmodel <- glm(Y1 ~ X1, family = poisson)
summary(poissonmodel)

# Test for overdispersion (dispersion and alpha parameters) from AER package
dispersiontest(poissonmodel)

# Zero-inflated negative binomial model coefficients
zinb <- zeroinfl(Y1 ~ X1 | X2, link = "logit", dist = "negbin")
summary(zinb)

#vuong test
vuong(zinb, poissonmodel)
