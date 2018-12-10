# install.packages("MASS")
library(MASS)
# install.packages("pscl")
library(pscl)
# insall.packages("AER")
library(AER)

mydata<- read.csv("C:\\Users\\win10\\Desktop\\quality.csv")
attach(mydata)

# Define variables
Y1 <- cbind(n_core_bugs)
Y2 <- cbind(n_user_bugs)

X1 <- cbind(n_non_bug_issues, proj_age, computed_n_stars, computed_n_forks, computed_n_src_loc, ci_integration)
X2 <- cbind(n_non_bug_issues, proj_age, computed_n_stars, computed_n_forks, computed_n_src_loc, ci_integration)

# Descriptive statistics
summary(Y1)
summary(Y2)
summary(X1)
summary(X2)

# Poisson model coefficients
poissonmodel <- glm(Y1 ~ X1, family = poisson)
summary(poissonmodel)

# Test for overdispersion (dispersion and alpha parameters) from AER package
dispersiontest(poissonmodel)

# Zero-inflated negative binomial model coefficients
zinb <- zeroinfl(Y1 ~ X1, link = "logit", dist = "negbin")
summary(zinb)

#vuong test
vuong(zinb, poissonmodel)
