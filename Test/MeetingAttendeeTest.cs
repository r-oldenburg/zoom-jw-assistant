using NUnit.Framework;
using ZoomJWAssistant.Models;

namespace ZoomJWAssistant.Test
{
    [TestFixture]
    public class MeetingAttendeeTest
    {
        [TestCase("Nachname, Vorname - 2", "Nachname, Vorname", 2)]
        [TestCase("Vorname Nachname -2", "Vorname Nachname", 2)]
        [TestCase("Vorname Nachname- 2", "Vorname Nachname", 2)]
        [TestCase("Vorname Nachname-2", "Vorname Nachname", 2)]
        [TestCase("Vorname Nachname / 2", "Vorname Nachname", 2)]
        [TestCase("Vorname Nachname /2", "Vorname Nachname", 2)]
        [TestCase("Vorname Nachname/ 2", "Vorname Nachname", 2)]
        [TestCase("Vorname Nachname2", "Vorname Nachname", 2)]
        [TestCase("Vorname Nachname  2", "Vorname Nachname", 2)]
        [TestCase("Vorname Nachname   -; 2", "Vorname Nachname", 2)]
        [TestCase("Vorname Nachname(2)", "Vorname Nachname", 2)]
        [TestCase("Vorname Nachname (2)", "Vorname Nachname", 2)]
        [TestCase("Vorname Nachname  (2)", "Vorname Nachname", 2)]
        [TestCase("Vorname Nachname  (2", "Vorname Nachname", 2)]
        [TestCase("Vorname Nachname (2", "Vorname Nachname", 2)]
        [TestCase("Vorname Nachname 2)", "Vorname Nachname", 2)]
        [TestCase("Vorname Nachname2)", "Vorname Nachname", 2)]
        public void NameWithAppendedNumberIsParsed(string incomingName, string expectedName, int expectedNum)
        {
            MeetingAttendee testee = new MeetingAttendee();
            testee.CurrentTechnicalName = incomingName;
            Assert.AreEqual(expectedName, testee.Name);
            Assert.AreEqual(expectedNum, testee.NumberOfPersons);
        }

        [TestCase("Vorname Nachname")]
        [TestCase("VornameNachname")]
        [TestCase("035443534")]
        [TestCase("035443534-22")]
        [TestCase("Call-In User_1")]
        [TestCase("Galaxy Tab A (2016)")]
        [TestCase("Pils Herbert147")]
        [TestCase("03544 3534")]
        [TestCase("sdfdsf 4545 dfd")]
        public void InvalidNamesAreNotParsedOrChanged(string incomingName)
        {
            MeetingAttendee testee = new MeetingAttendee();
            testee.CurrentTechnicalName = incomingName;
            Assert.AreEqual(incomingName, testee.Name);
            Assert.AreEqual(1, testee.NumberOfPersons);
        }

        [TestCase("(2) Vorname Nachname", "Vorname Nachname", 2)]
        [TestCase("(2 Vorname Nachname", "Vorname Nachname", 2)]
        [TestCase("2) Vorname Nachname", "Vorname Nachname", 2)]
        [TestCase("(2)  Vorname Nachname", "Vorname Nachname", 2)]
        [TestCase("2- Vorname Nachname", "Vorname Nachname", 2)]
        [TestCase("2 - Vorname Nachname", "Vorname Nachname", 2)]
        [TestCase("2  - Vorname Nachname", "Vorname Nachname", 2)]
        [TestCase("2-Vorname Nachname", "Vorname Nachname", 2)]
        [TestCase("2 -  Vorname Nachname", "Vorname Nachname", 2)]
        [TestCase("2Vorname Nachname", "Vorname Nachname", 2)]
        public void NameWithPreendedNumberIsParsed(string incomingName, string expectedName, int expectedNum)
        {
            MeetingAttendee testee = new MeetingAttendee();
            testee.CurrentTechnicalName = incomingName;
            Assert.AreEqual(expectedName, testee.Name);
            Assert.AreEqual(expectedNum, testee.NumberOfPersons);
        }
    }
}
