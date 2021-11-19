using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading; //주미가 새로넣음
using System.Threading.Tasks;
using System.Windows.Forms;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        public struct listArr
        {
            public string Idx;
            public string Subject;
            public string Kind;
            public string Title;
            public string Date;
        }
        public listArr[] form2List = new listArr[200];
        public int dataCount = 0;  //form2List의 인덱스
        public string st = "";
        public Form1()
        {
            InitializeComponent();
        }

        private static bool WaitForVisivle(IWebDriver driver, By by)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

            try
            {
                IWebElement element = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(by));
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }


        private void downloadButton_Click(object sender, EventArgs e)
        {

            if (idInput.Text.Length == 0)
            {
                MessageBox.Show("ID가 입력되지 않았습니다. 입력해주세요.", "LMS 학습보조 RPA 시스템", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); //알림창
                return;
            }
            if (pwInput.Text.Length == 0)
            {
                MessageBox.Show("비밀번호가 입력되지 않았습니다. 입력해주세요.", "LMS 학습보조 RPA 시스템", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); //알림창
                return;
            }
            if (subjectInput.Text.Length == 0)
            {
                MessageBox.Show("과목명이 입력되지 않았습니다. 입력해주세요.", "LMS 학습보조 RPA 시스템", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); //알림창
                return;
            }
            if (!checkBox1.Checked && !checkBox2.Checked && !checkBox3.Checked)
            {
                MessageBox.Show("아무런 항목도 선택되지 않았습니다. 선택해주세요.", "LMS 학습보조 RPA 시스템", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); //알림창
                return;
            }
            ChromeOptions options = new ChromeOptions();
            ChromeDriverService driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;

            ChromeDriver driver = new ChromeDriver(driverService, options);
            driver.Url = "https://eclass.donga.ac.kr/xn-sso/login.php?auto_login=&sso_only=&cvs_lgn=&return_url=https%3A%2F%2Feclass.donga.ac.kr%2Fxn-sso%2Fgw-cb.php%3Ffrom%3D%26login_type%3Dstandalone%26return_url%3Dhttps%253A%252F%252Feclass.donga.ac.kr%252Flogin%252Fcallback";
            IWebElement id = driver.FindElementById("login_user_id");
            id.SendKeys(idInput.Text);
            IWebElement pw = driver.FindElementById("login_user_password");
            pw.SendKeys(pwInput.Text);

            IWebElement wrapper = driver.FindElementById("login_wapper");
            driver.FindElementByXPath("//*[@id='form1']/div[4]/a").Click();

            //로그인 체크
            string myTitle = driver.Title;
            if (myTitle == "동아대학교 LMS")  //로그인 성공
            {
                //LMS이동
                driver.FindElementByCssSelector("#visual > div > div.xn-main-login-container > div:nth-child(2) > div.xn-main-link-wrap.xn-main-lms-link-wrap > a").Click();
            }
            else  //로그인 실패
            {
                driver.Close();
                MessageBox.Show("로그인에 실패했습니다.\n아이디,비번을 다시 입력해주세요.", "LMS 학습보조 RPA 시스템", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); //알림창
                return;
            }

            var dashboardCard = driver.FindElements(By.ClassName("ic-DashboardCard__header-subtitle"));

            string findSubject = (subjectInput.Text).ToString();

            if (subjectInput.Text.Equals("전체"))
            {
                var links = driver.FindElements(By.ClassName("ic-DashboardCard__link"));
                var currentUrl = driver.Url; //강의자료실 들어가기전 현재 url
                for (int i = 0; i < links.Count(); i++)
                {
                    string subject = (driver.FindElements(By.ClassName("ic-DashboardCard__header-term"))[i].Text).ToString(); //과목에 비교과가 들어가는지 체크
                    if (subject.Contains("비교과") == false && subject.Contains("학기") == true)
                    {
                        driver.FindElements(By.ClassName("ic-DashboardCard__link"))[i].Click(); //해당하는 과목으로 들어가기

                        var subjectTitle = driver.FindElements(By.ClassName("ellipsible"))[1]; //강의 제목
                        st = subjectTitle.Text;

                        if (checkBox1.Checked)
                        {
                            var referenceRoom = driver.FindElements(By.ClassName("context_external_tool_2"));
                            referenceRoom[0].Click(); //강의자료실로 이동


                            string winHandleBefore = driver.CurrentWindowHandle;

                            IWebElement iFrame = driver.FindElementById("tool_content");
                            driver.SwitchTo().Frame(iFrame);

                            var data = driver.FindElements(By.ClassName("xn-resource-item-container")); //강의 자료들

                            var chk = false;    //한번 다운받고 나온건지 체크

                            for (int j = 0; j < data.Count(); j++)
                            {
                                int j_plus = j + 1;

                                if (chk)
                                {
                                    winHandleBefore = driver.CurrentWindowHandle;

                                    iFrame = driver.FindElementById("tool_content");
                                    driver.SwitchTo().Frame(iFrame);

                                    data = driver.FindElements(By.ClassName("xn-resource-item-container")); //강의 자료들
                                }
                                chk = false;

                                if (driver.FindElementByCssSelector("#xn-course-resource > div.xn-viewmode-course-resource-design > div > div > div.xnvmrd-resource-container > div:nth-child(" + j_plus + ") > div > div > div.xnri-description.pdf > div.xnri-description-learn-header > div.xnci-component-complete-container > span").Text == "미완료")
                                {   //강의 자료 다운로드가 완료일 때
                                    chk = true;

                                    driver.FindElements(By.ClassName("xn-resource-item"))[j].Click();   //해당 강의자료 들어와서

                                    Thread.Sleep(5000);

                                    string winHandleBeforeData = driver.CurrentWindowHandle;
                                    IWebElement iFrameData = driver.FindElements(By.ClassName("xnrb-viewer-frame"))[0];
                                    driver.SwitchTo().Frame(iFrameData);   //아이프레임 들어가기

                                    string winHandleBeforeData2 = driver.CurrentWindowHandle;
                                    IWebElement iFrameData2 = driver.FindElements(By.ClassName("xn-content-frame"))[0];
                                    driver.SwitchTo().Frame(iFrameData2);   //아이프레임 들어가기

                                    driver.FindElementByCssSelector("#play-controller > div.vc-over-layer-main-controller.vc-controller-over-layer > div > div > div.vc-pctrl-right-control-wrapper > div.vc-btn.vc-pctrl-download-btn").Click();   //다운로드받기

                                    Thread.Sleep(2000);

                                    driver.SwitchTo().Window(winHandleBeforeData2);
                                    driver.SwitchTo().Window(winHandleBeforeData);

                                    string winHandleBefore3 = driver.CurrentWindowHandle;
                                    IWebElement iFrame3 = driver.FindElementById("tool_content");
                                    driver.SwitchTo().Frame(iFrame3);

                                    string title = driver.FindElementByCssSelector("#xn-resource-view > div > div.xn-resource-body > div.xn-component-title > span").Text.ToString();

                                    form2List[dataCount].Subject = st;
                                    form2List[dataCount].Kind = "강의자료";
                                    form2List[dataCount].Title = title;
                                    form2List[dataCount].Date = " ";
                                    dataCount++;

                                    driver.FindElementByCssSelector("#xn-resource-view > div > div.xnrv-top-container > button").Click(); //다운다받고 목록으로 돌아가기
                                    Thread.Sleep(2000);
                                    driver.SwitchTo().Window(winHandleBefore3);
                                }
                            }

                            driver.SwitchTo().Window(winHandleBefore);
                        }

                        if (checkBox2.Checked)
                        {
                            // 강의콘텐츠로 이동
                            var referenceContent = driver.FindElements(By.ClassName("context_external_tool_1"));
                            referenceContent[0].Click();

                            //모든 주차 펼치기
                            string winHandleBefore4 = driver.CurrentWindowHandle;

                            IWebElement iFrameContent = driver.FindElementById("tool_content");
                            driver.SwitchTo().Frame(iFrameContent);

                            WaitForVisivle(driver, By.CssSelector("#xn-course-learn > div > div.xncl-section-top-container > div.xncl-btn-container > button"));

                            var button = driver.FindElement(By.ClassName("xncl-btn-unfold-sections"));
                            button.SendKeys(OpenQA.Selenium.Keys.Enter);

                            var item = driver.FindElements(By.ClassName("xn-component-item"));

                            //과제, 마감일 체크
                            for (int j = 0; j < item.Count(); j++)
                            {

                                if (item[j].GetAttribute("class").ToString().Contains("assignment") == true)
                                {
                                    string title = item[j].GetAttribute("aria-label").ToString();
                                    string date = item[j].FindElements(By.ClassName("top-value"))[0].Text;
                                    char space = ' ';

                                    //마감일 문자열 자르기
                                    string[] spstring = date.Split(space);
                                    int monthCheck = Convert.ToInt32((spstring[0].Substring(0, spstring[0].Length - 1))); //마감일 월
                                    int dayCheck = Convert.ToInt32((spstring[1].Substring(0, spstring[1].Length - 1))); //마감일 일

                                    //현재날짜
                                    string monthNow = (DateTime.Now.ToString("MM"));
                                    string dayNow = DateTime.Now.ToString("dd");

                                    //마감일 현재날짜 비교
                                    if (Convert.ToInt32(monthNow) <= monthCheck)
                                    {
                                        if (Convert.ToInt32(dayNow) <= dayCheck)
                                        {
                                            form2List[dataCount].Subject = st;
                                            form2List[dataCount].Kind = "과제";
                                            form2List[dataCount].Title = title;
                                            form2List[dataCount].Date = date;
                                            dataCount++;
                                        }
                                    }
                                }
                            }
                            driver.SwitchTo().Window(winHandleBefore4);
                        }

                        if (checkBox3.Checked)
                        {
                            if (checkBox1.Checked || checkBox2.Checked)
                            {
                                var home = driver.FindElements(By.ClassName("home"));
                                home[1].Click();
                            }

                            Thread.Sleep(1000);
                            var divC = driver.FindElements(By.ClassName("ic-announcement-row"));

                            for (int t = 0; t < divC.Count(); t++)
                            {

                                var tmp = divC[t].FindElement(By.TagName("span"));
                                var test = tmp.FindElement(By.TagName("span"));
                                if (test.GetAttribute("class").Contains("ChHxxOF") == true)
                                {  //아직 안본공지사항일때
                                    var linkC = divC[t].FindElements(By.ClassName("ic-item-row__content-link-container"));
                                    var title = linkC[0].FindElement(By.TagName("h3")).Text;
                                    var content = linkC[1].FindElement(By.TagName("div")).Text;

                                    MessageBox.Show(content, title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                }
                            }
                        }

                        //다돌고나면
                        driver.Url = currentUrl; //원래 대시보드로 돌아가기
                        Thread.Sleep(1000); //1초의 유예시간
                    }
                    else
                    {
                        continue;
                    }
                }

            }
            else
            {
                Boolean subjectCheck = false;
                for (int i = 0; i < dashboardCard.Count(); i++)
                {
                    string subject = (driver.FindElements(By.ClassName("ic-DashboardCard__header-subtitle"))[i].Text).ToString();
                    if (subject.Contains(findSubject) == true)
                    {
                        subjectCheck = true;
                        driver.FindElements(By.ClassName("ic-DashboardCard__link"))[i].Click(); //해당하는 과목으로 들어가기

                        var subjectTitle = driver.FindElements(By.ClassName("ellipsible"))[1]; //강의 제목
                        st = subjectTitle.Text;

                        if (checkBox1.Checked)
                        {
                            var referenceRoom = driver.FindElements(By.ClassName("context_external_tool_2"));
                            referenceRoom[0].Click(); //강의자료실로 이동


                            string winHandleBefore = driver.CurrentWindowHandle;

                            IWebElement iFrame = driver.FindElementById("tool_content");
                            driver.SwitchTo().Frame(iFrame);

                            var data = driver.FindElements(By.ClassName("xn-resource-item-container")); //강의 자료들

                            var chk = false;    //한번 다운받고 나온건지 체크

                            for (int j = 0; j < data.Count(); j++)
                            {
                                int j_plus = j + 1;

                                if (chk)
                                {
                                    winHandleBefore = driver.CurrentWindowHandle;

                                    iFrame = driver.FindElementById("tool_content");
                                    driver.SwitchTo().Frame(iFrame);

                                    data = driver.FindElements(By.ClassName("xn-resource-item-container")); //강의 자료들
                                }
                                chk = false;

                                if (driver.FindElementByCssSelector("#xn-course-resource > div.xn-viewmode-course-resource-design > div > div > div.xnvmrd-resource-container > div:nth-child(" + j_plus + ") > div > div > div.xnri-description.pdf > div.xnri-description-learn-header > div.xnci-component-complete-container > span").Text == "미완료")
                                {   //강의 자료 다운로드가 완료일 때
                                    chk = true;

                                    driver.FindElements(By.ClassName("xn-resource-item"))[j].Click();   //해당 강의자료 들어와서

                                    Thread.Sleep(5000);

                                    string winHandleBeforeData = driver.CurrentWindowHandle;
                                    IWebElement iFrameData = driver.FindElements(By.ClassName("xnrb-viewer-frame"))[0];
                                    driver.SwitchTo().Frame(iFrameData);   //아이프레임 들어가기

                                    string winHandleBeforeData2 = driver.CurrentWindowHandle;
                                    IWebElement iFrameData2 = driver.FindElements(By.ClassName("xn-content-frame"))[0];
                                    driver.SwitchTo().Frame(iFrameData2);   //아이프레임 들어가기

                                    driver.FindElementByCssSelector("#play-controller > div.vc-over-layer-main-controller.vc-controller-over-layer > div > div > div.vc-pctrl-right-control-wrapper > div.vc-btn.vc-pctrl-download-btn").Click();   //다운로드받기

                                    Thread.Sleep(2000);

                                    driver.SwitchTo().Window(winHandleBeforeData2);
                                    driver.SwitchTo().Window(winHandleBeforeData);

                                    string winHandleBefore3 = driver.CurrentWindowHandle;
                                    IWebElement iFrame3 = driver.FindElementById("tool_content");
                                    driver.SwitchTo().Frame(iFrame3);

                                    string title = driver.FindElementByCssSelector("#xn-resource-view > div > div.xn-resource-body > div.xn-component-title > span").Text.ToString();

                                    form2List[dataCount].Subject = st;
                                    form2List[dataCount].Kind = "강의자료";
                                    form2List[dataCount].Title = title;
                                    form2List[dataCount].Date = " ";
                                    dataCount++;

                                    driver.FindElementByCssSelector("#xn-resource-view > div > div.xnrv-top-container > button").Click(); //다운다받고 목록으로 돌아가기
                                    Thread.Sleep(2000);
                                    driver.SwitchTo().Window(winHandleBefore3);
                                }
                            }

                            driver.SwitchTo().Window(winHandleBefore);
                        }

                        if (checkBox2.Checked)
                        {
                            // 강의콘텐츠로 이동
                            var referenceContent = driver.FindElements(By.ClassName("context_external_tool_1"));
                            referenceContent[0].Click();

                            //모든 주차 펼치기
                            string winHandleBefore4 = driver.CurrentWindowHandle;

                            IWebElement iFrameContent = driver.FindElementById("tool_content");
                            driver.SwitchTo().Frame(iFrameContent);

                            WaitForVisivle(driver, By.CssSelector("#xn-course-learn > div > div.xncl-section-top-container > div.xncl-btn-container > button"));

                            var button = driver.FindElement(By.ClassName("xncl-btn-unfold-sections"));
                            button.SendKeys(OpenQA.Selenium.Keys.Enter);

                            var item = driver.FindElements(By.ClassName("xn-component-item"));

                            //과제, 마감일 체크
                            for (int j = 0; j < item.Count(); j++)
                            {

                                if (item[j].GetAttribute("class").ToString().Contains("assignment") == true)
                                {
                                    string title = item[j].GetAttribute("aria-label").ToString();
                                    string date = item[j].FindElements(By.ClassName("top-value"))[0].Text;
                                    char space = ' ';

                                    //마감일 문자열 자르기
                                    string[] spstring = date.Split(space);
                                    int monthCheck = Convert.ToInt32((spstring[0].Substring(0, spstring[0].Length - 1))); //마감일 월
                                    int dayCheck = Convert.ToInt32((spstring[1].Substring(0, spstring[1].Length - 1))); //마감일 일

                                    //현재날짜
                                    string monthNow = (DateTime.Now.ToString("MM"));
                                    string dayNow = DateTime.Now.ToString("dd");

                                    //마감일 현재날짜 비교
                                    if (Convert.ToInt32(monthNow) <= monthCheck)
                                    {
                                        if (Convert.ToInt32(dayNow) <= dayCheck)
                                        {
                                            form2List[dataCount].Subject = st;
                                            form2List[dataCount].Kind = "과제";
                                            form2List[dataCount].Title = title;
                                            form2List[dataCount].Date = date;
                                            dataCount++;
                                        }
                                    }
                                }
                            }
                            driver.SwitchTo().Window(winHandleBefore4);
                        }

                        if (checkBox3.Checked)
                        {
                            if (checkBox1.Checked || checkBox2.Checked)
                            {
                                var home = driver.FindElements(By.ClassName("home"));
                                home[1].Click();
                            }
                            Thread.Sleep(1000);
                            var divC = driver.FindElements(By.ClassName("ic-announcement-row"));

                            for (int t = 0; t < divC.Count(); t++)
                            {

                                var tmp = divC[t].FindElement(By.TagName("span"));
                                var test = tmp.FindElement(By.TagName("span"));
                                if (test.GetAttribute("class").Contains("ChHxxOF") == true)
                                {  //아직 안본공지사항일때
                                    var linkC = divC[t].FindElements(By.ClassName("ic-item-row__content-link-container"));
                                    var title = linkC[0].FindElement(By.TagName("h3")).Text;
                                    var content = linkC[1].FindElement(By.TagName("div")).Text;

                                    MessageBox.Show(content, title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                                }

                            }
                        }
                        //다 돌고나면 브레이크해주기(강의자료 다운로드, 과제체크끝나면)
                        break;
                    }
                }
                if (!subjectCheck)
                {

                    MessageBox.Show("해당 과목이 없습니다.\n과목명을 다시 입력해주세요.", "LMS 학습보조 RPA 시스템", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); //알림창
                    driver.Close();
                    return;
                }
            }

            Form2 showForm = new Form2(form2List, dataCount, driver);
            showForm.Show();


        }

    }
}