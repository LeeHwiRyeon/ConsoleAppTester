﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MiniGameTC {
    public class TCRunner {
        private List<ITestScenario> m_testCases = new List<ITestScenario>();

        public void Init()
        {
            var testCaseTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(type => type.GetInterfaces().Contains(typeof(ITestScenario))
                                && type.IsDefined(typeof(IgnoreScenarioAttribute), inherit: false) == false)
                .ToArray();


            m_testCases.Clear();
            foreach (var type in testCaseTypes) {
                if (Activator.CreateInstance(type) is ITestScenario testCase) {
                    m_testCases.Add(testCase);
                }
            }
        }

        public void RunAllTests()
        {
            if (m_testCases == null || m_testCases.Count == 0) {
                Console.WriteLine("No test cases found!");
                return;
            }
            var reporter = new Reporter();
            int successCount = 0;
            int failureCount = 0;
            foreach (var tc in m_testCases) {
                var result = RunTestCase(tc);
                if (result.Status == "Success") {
                    successCount++;
                } else {
                    failureCount++;
                }
                reporter.AddReport(result);
            }
            reporter.PrintAllReports();

            var msg = $"테스트 완료: 성공({successCount}) 실패({failureCount})";
            SlackNotifier.SendMessage(msg);
            Console.WriteLine(msg);
        }

        private TestScenarioReport RunTestCase(ITestScenario tc)
        {
            tc.OnInitialize();
            var result = tc.ExecuteScenario();
            tc.OnFinalize();
            return result;
        }
    }
}
