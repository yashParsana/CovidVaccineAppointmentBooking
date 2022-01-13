using System;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Xml.Serialization;
using FinalProject;
using System.Linq;
using System.Windows.Media;

namespace FinalAssignment
{
    enum VaccineOptions
    {
        JohnsonAndJohnson = 1,
        Moderna = 2,
        Pfizer = 3,
        AstraZeneca = 4,
    }

    enum IdentificationDocumentOptions
    {
        Passport,
        DrivingLicense,
        HealthCard
    }

    public partial class MainWindow : Window
    {
        String fileName = "VaccineAppointment.xml";
        SlotSelection slotSelection;
        SlotSelection slotSelectionFromXML = new SlotSelection();
        int selectedVaccineIndex = 0;
        int selectedDoseIndex = 0;

        public MainWindow()
        {
            InitializeComponent();
            AddVaccineInfo();
            LoadDetailsFromXML();
            AddTimeSlots();
            AddDoseDetails();
            AddIdentificationProof();
        }

        //Add vaccines in Radio buttons using Enum
        public void AddVaccineInfo()
        {
            var vaccine = Enum.GetValues(typeof(VaccineOptions));
            for (int i = 0; i < vaccine.Length; i++)
            {
                RadioButton rb = new RadioButton() { Content = vaccine.GetValue(i), IsChecked = false, GroupName = "Vaccines", Name = string.Format("{0}", vaccine.GetValue(i)), Tag = i + 1 };
                rb.Click += VaccineSelected;
                approvedVaccinePanel.Children.Add(rb);
            }
        }


        //Add Doses in Radio buttons
        public void AddDoseDetails()
        {
            RadioButton rb = new RadioButton() { Content = "1st Dose", IsChecked = false, GroupName = "Doses", Name = "FirstDose", Tag = "1" };
            rb.Click += DoseSelected;
            doseWantToTakePanel.Children.Add(rb);

            rb = new RadioButton() { Content = "2nd Dose", IsChecked = false, GroupName = "Doses", Name = "SecondDose", Tag = "2" };
            rb.Click += DoseSelected;
            doseWantToTakePanel.Children.Add(rb);
        }

        //Add Identification proofs in Drop Down from the Enum
        public void AddIdentificationProof()
        {
            var IdentificationDocument = Enum.GetValues(typeof(IdentificationDocumentOptions));
            for (int i = 0; i < IdentificationDocument.Length; i++)
            {
                identificationDocumentOptions.Items.Add(IdentificationDocument.GetValue(i));
            }
        }

        void VaccineSelected(object sender, RoutedEventArgs e)
        {
            //Get the selected Vaccine Index.
            selectedVaccineIndex = int.Parse((sender as RadioButton).Tag.ToString());
        }

        void DoseSelected(object sender, RoutedEventArgs e)
        {
            //Get the Selected Dose Index(1st or 2nd Dose)
            selectedDoseIndex = int.Parse((sender as RadioButton).Tag.ToString());
        }

        //Search from the list by First Name.
        //
        private void SearchByName(object sender, RoutedEventArgs e)
        {
            string firstName = inputSearchByName.Text;

            if (firstName.Length > 0)
            {
                var query = from slot in slotSelection.List
                            where slot.VaccinationDetail.FirstName.ToLower().Trim().Contains(firstName.ToLower().Trim())
                            select slot;
                apointmentDataGridView.ItemsSource = query;
            }
            else
            {
                apointmentDataGridView.ItemsSource = slotSelection.List;//If search input text length is 0. Display whole list.
            }
        }

        //Load when UI is loaded for the first time to store bookings in init from the XML file.
        private void LoadDetailsFromXML()
        {
            XmlSerializer serializer = null;
            StreamReader reader = null;
            try
            {
                slotSelection = new SlotSelection();
                slotSelectionFromXML = new SlotSelection();
                if (File.Exists(fileName))
                {
                    serializer = new XmlSerializer(typeof(SlotSelection));
                    reader = new StreamReader(fileName);
                    slotSelection = (SlotSelection)serializer.Deserialize(reader);
                    slotSelectionFromXML = slotSelection;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                if (reader != null)
                {
                    reader.Close();
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
        }

        //This will create slots for the day
        //Slot is available after every 20 minutes from the 10:00 AM - 05:00 PM.
        public void AddTimeSlots()
        {
            DateTime currentTime = DateTime.Now;
            DateTime workStartTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 10, 0, 0);
            TimeSlotSelection timeSlot = null;

            for (int i = 0; i < 21; i++)
            {
                timeSlot = new TimeSlotSelection();
                timeSlot.ReservationTime = workStartTime.ToString("hh:mm tt");
                timeSlot.IsSlotReserved = false;
                workStartTime = workStartTime.AddMinutes(20);
                timeSlotItems.Items.Add(timeSlot.ReservationTime);
                foreach (TimeSlotSelection slot in slotSelectionFromXML.List)
                {
                    if (timeSlot.ReservationTime == slot.ReservationTime)
                    {
                        timeSlot.IsSlotReserved = true;
                        timeSlotItems.Items.Remove(slot.ReservationTime.ToString());
                    }
                }
            }
            timeSlotItems.SelectedIndex = -1;//Default selected value is blank.
        }

        //Display booked appointments when clicked on Display Appointments
        private void DisplayBookedSlots(object sender, RoutedEventArgs e)
        {
            XmlSerializer serializer = null;
            StreamReader reader = null;
            try
            {
                if (File.Exists(fileName))
                {
                    serializer = new XmlSerializer(typeof(SlotSelection));
                    reader = new StreamReader(fileName);
                    slotSelection = (SlotSelection)serializer.Deserialize(reader);
                    apointmentDataGridView.ItemsSource = slotSelection.List;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
        }

        //Book appointment
        private void BookVaccinationSlot(object sender, RoutedEventArgs e)
        {
            VaccinationDetails vaccinationDetail = null;
            TimeSlotSelection timeSlot = new TimeSlotSelection();
            if (CheckValidation())//Check for the validation rule.
            {
                string firstName = inputFirstName.Text;
                string lastName = inputLastName.Text;
                int age = int.Parse(inputAge.Text);
                decimal mobileNumber = decimal.Parse(inputMobileNumber.Text);
                string selectedIdentityDocument = identificationDocumentOptions.SelectedItem.ToString();
                string identityDocumentNumber = inputIdentityNumber.Text;
                string selectedTime = timeSlotItems.SelectedItem.ToString();

                switch (selectedVaccineIndex)
                {
                    //Polymorphism is applied here.
                    case (int)VaccineOptions.JohnsonAndJohnson:
                        vaccinationDetail = new JohnsonAndJohnson(firstName, lastName, age, mobileNumber, selectedIdentityDocument, identityDocumentNumber, "Johnson & Johnson", selectedDoseIndex);
                        break;

                    case (int)VaccineOptions.Moderna:
                        vaccinationDetail = new Moderna(firstName, lastName, age, mobileNumber, selectedIdentityDocument, identityDocumentNumber, "Moderna", selectedDoseIndex);
                        break;

                    case (int)VaccineOptions.Pfizer:
                        vaccinationDetail = new Pfizer(firstName, lastName, age, mobileNumber, selectedIdentityDocument, identityDocumentNumber, "Pfizer", selectedDoseIndex);
                        break;

                    case (int)VaccineOptions.AstraZeneca:
                        vaccinationDetail = new AstraZeneca(firstName, lastName, age, mobileNumber, selectedIdentityDocument, identityDocumentNumber, "AstraZeneca", selectedDoseIndex);
                        break;

                    default:
                        Console.WriteLine("Something went wrong!");
                        break;
                }
                timeSlot.ReservationTime = selectedTime;
                timeSlot.VaccinationDetail = vaccinationDetail;
                timeSlot.IsSlotReserved = true;
                slotSelection.Add(timeSlot);

                timeSlotItems.Items.Remove(timeSlotItems.SelectedValue.ToString());
                ReserveSlot();

                ClearAllFields();
            }
        }

        private void ClearAllFields()
        {
            inputFirstName.Text = string.Empty;
            inputLastName.Text = string.Empty;
            inputAge.Text = string.Empty;
            inputMobileNumber.Text = string.Empty;
            inputIdentityNumber.Text = string.Empty;

            foreach (RadioButton rb in approvedVaccinePanel.Children.OfType<RadioButton>())
            {
                rb.IsChecked = false;
            }

            foreach (RadioButton rb in doseWantToTakePanel.Children.OfType<RadioButton>())
            {
                rb.IsChecked = false;
            }

            for (int i = 0; i < slotSelection.Count; i++)
            {
                if (slotSelection[i].IsSlotReserved == true)
                {
                    timeSlotItems.Items.Remove(slotSelection[i].ReservationTime);
                }
                timeSlotItems.SelectedIndex = -1;
            }
            identificationDocumentOptions.SelectedIndex = -1;
        }

        //Write data in XML file once it is added in list.
        private void ReserveSlot()
        {
            XmlSerializer serializer = null;
            TextWriter writer = null;
            try
            {
                serializer = new XmlSerializer(typeof(SlotSelection));
                writer = new StreamWriter(fileName);
                serializer.Serialize(writer, slotSelection);
                writer.Close();
                apointmentDataGridView.ItemsSource = slotSelection.List;
                MessageBox.Show("Appointment booked successfully!");
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
                if (writer != null)
                {
                    writer.Close();
                }
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
            }
        }

        //Check for the validation for all the fields.
        private bool CheckValidation()
        {
            bool valid = true;

            if (timeSlotItems.SelectedIndex == -1)
            {
                timeSlotError.Visibility = Visibility.Visible;
                valid = false;
                timeSlotError.Foreground = Brushes.Red;
            }
            else
            {
                timeSlotError.Visibility = Visibility.Hidden;
                timeSlotError.Foreground = Brushes.Black;
            }

            int countSelectedValue = 0;
            foreach (RadioButton rb in approvedVaccinePanel.Children.OfType<RadioButton>())
            {
                if (rb.IsChecked == true)
                {
                    countSelectedValue++;
                }
            }

            if (countSelectedValue == 0)
            {
                errorVaccineSelection.Visibility = Visibility.Visible;
                valid = false;
                errorVaccineSelection.Foreground = Brushes.Red;
            }
            else
            {
                errorVaccineSelection.Visibility = Visibility.Hidden;
                errorVaccineSelection.Foreground = Brushes.Black;
            }

            countSelectedValue = 0;
            foreach (RadioButton rb in doseWantToTakePanel.Children.OfType<RadioButton>())
            {
                if (rb.IsChecked == true)
                {
                    countSelectedValue++;
                }
            }

            if (countSelectedValue == 0)
            {
                errorDoseWantToTake.Visibility = Visibility.Visible;
                valid = false;
                errorDoseWantToTake.Foreground = Brushes.Red;
            }
            else
            {
                errorDoseWantToTake.Visibility = Visibility.Hidden;
                errorDoseWantToTake.Foreground = Brushes.Black;
            }

            if (selectedVaccineIndex == 1 && selectedDoseIndex == 2)
            {
                errorDoseWantToTake.Visibility = Visibility.Visible;
                valid = false;
                errorDoseWantToTake.Content = "You don't need to take second dose.";
                errorDoseWantToTake.Foreground = Brushes.Red;
            }
            else
            {
                lblErrorFirstName.Visibility = Visibility.Hidden;
                lblErrorFirstName.Foreground = Brushes.Black;
            }

            if (inputFirstName.Text == string.Empty)
            {
                lblErrorFirstName.Visibility = Visibility.Visible;
                valid = false;
                lblErrorFirstName.Foreground = Brushes.Red;
            }
            else
            {
                lblErrorFirstName.Visibility = Visibility.Hidden;
                lblErrorFirstName.Foreground = Brushes.Black;
            }

            if (inputLastName.Text == string.Empty)
            {
                lblErrorLastName.Visibility = Visibility.Visible;
                valid = false;
                lblErrorLastName.Foreground = Brushes.Red;
            }
            else
            {
                lblErrorLastName.Visibility = Visibility.Hidden;
                lblErrorLastName.Foreground = Brushes.Black;
            }

            if (inputMobileNumber.Text == string.Empty || !decimal.TryParse(inputMobileNumber.Text, out decimal mobileNumber) || inputMobileNumber.Text.Length != 10)
            {
                lblErrorMobileNumber.Visibility = Visibility.Visible;
                valid = false;
                lblErrorMobileNumber.Foreground = Brushes.Red;
            }
            else
            {
                lblErrorMobileNumber.Visibility = Visibility.Hidden;
                lblErrorMobileNumber.Foreground = Brushes.Black;
            }

            if (inputAge.Text == string.Empty || !decimal.TryParse(inputAge.Text, out decimal personAge) || personAge < 18 || personAge > 100)
            {
                lblErrorAge.Visibility = Visibility.Visible;
                valid = false;
                lblErrorAge.Foreground = Brushes.Red;
            }
            else
            {
                lblErrorAge.Visibility = Visibility.Hidden;
                lblErrorAge.Foreground = Brushes.Black;
            }

            if (identificationDocumentOptions.SelectedIndex == -1)
            {
                verficationDocumentSelectionError.Visibility = Visibility.Visible;
                valid = false;
                verficationDocumentSelectionError.Foreground = Brushes.Red;
            }
            else
            {
                verficationDocumentSelectionError.Visibility = Visibility.Hidden;
                verficationDocumentSelectionError.Foreground = Brushes.Black;
            }
            
            //If Johnson & Johnson is selected and select for the second dose show an error message.
            if (inputIdentityNumber.Text == string.Empty || (identificationDocumentOptions.SelectedIndex != -1 && !VerifyIdentificatioNumber(identificationDocumentOptions.SelectedIndex, inputIdentityNumber.Text)))
            {
                lblErrorIdentityNumber.Visibility = Visibility.Visible;
                valid = false;
                lblErrorIdentityNumber.Foreground = Brushes.Red;
            }
            else
            {
                lblErrorIdentityNumber.Visibility = Visibility.Hidden;
                lblErrorIdentityNumber.Foreground = Brushes.Black;
            }
            return valid;
        }

        //Check validation for different documents
        private bool VerifyIdentificatioNumber(int selectedIndex, string inputIdentityNumber)
        {
            bool isValidDocument = false;
            switch (selectedIndex)
            {
                case (int)IdentificationDocumentOptions.Passport:
                    //8 Character passport number
                    if (inputIdentityNumber.Length == 8)
                    {
                        isValidDocument = true;
                        lblErrorIdentityNumber.Content = "Passport should have 8 characters";
                    }
                    break;

                case (int)IdentificationDocumentOptions.HealthCard:
                    //Health card with 12 characters
                    if (inputIdentityNumber.Length == 12)
                    {
                        isValidDocument = true;
                        lblErrorIdentityNumber.Content = "Health card should have 12 characters";
                    }
                    break;

                case (int)IdentificationDocumentOptions.DrivingLicense:
                    //15 character number Driving License
                    if (inputIdentityNumber.Length == 15)
                    {
                        isValidDocument = true;
                        lblErrorIdentityNumber.Content = "Driving License should have 15 characters";
                    }
                    break;

                default:
                    isValidDocument = false;
                    Console.WriteLine("Something went wrong!");
                    break;
            }
            return isValidDocument;
        }
    }
}
