﻿using AntonieMotors_XBCAD7319.Models;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Globalization;
using System.Net;

namespace AntonieMotors_XBCAD7319.Controllers
{
    public class AdminController : Controller
    {

        private readonly FirebaseAuthProvider _authProvider;
        private readonly FirebaseClient _firebaseClient;
        string businessId = BusinessID.businessId;



        public AdminController(IConfiguration configuration)
        {
            string apiKey = configuration["Firebase:ApiKey"];
            string databaseUrl = configuration["Firebase:DatabaseUrl"];

            // Initialize Firebase objects
            _authProvider = new FirebaseAuthProvider(new FirebaseConfig(apiKey));
            _firebaseClient = new FirebaseClient(databaseUrl);
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> QuoteGenerator()
        {
            await fetchQuoteRequests();
            return View();
        }

        public async Task<IActionResult> CustomerManagement(string searchQuery = "")
        {
            string businessId = BusinessID.businessId; // Replace with the logged-in user's business ID
            string managerId = BusinessID.userId; // Replace with the logged-in user's manager ID

            Console.WriteLine($"BusinessID: {businessId}, ManagerID: {managerId}");

            // Fetch customers asynchronously
            var customers = await GetCustomersAsync(businessId, managerId);

            // Filter customers based on the search query (case-insensitive)
            if (!string.IsNullOrEmpty(searchQuery))
            {
                customers = customers.Where(c =>
                    c.CustomerName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                    c.CustomerSurname.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                    c.CustomerEmail.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                    c.CustomerMobileNum.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            return View(customers);
        }

        private async Task<List<CustomerModel>> GetCustomersAsync(string businessId, string managerId)
        {
            try
            {
                // Fetch customers under the businessId
                var customers = await _firebaseClient
                    .Child($"Users/{businessId}/Customers")
                    .OnceAsync<CustomerModel>();

                Console.WriteLine($"Fetched {customers.Count} customers from Firebase.");

                // Filter customers by managerId if applicable
                var filteredCustomers = customers

                    .Select(c => new CustomerModel
                    {
                        CustomerName = c.Object.CustomerName,
                        CustomerSurname = c.Object.CustomerSurname,
                        CustomerEmail = c.Object.CustomerEmail,
                        CustomerMobileNum = c.Object.CustomerMobileNum,
                        CustomerAddress = c.Object.CustomerAddress,
                        CustomerType = c.Object.CustomerType
                    })
                    .ToList();

                // Console.WriteLine($"Filtered {filteredCustomers.Count} customers for ManagerID: {managerId}");
                return filteredCustomers;
            }
            catch (Exception ex)
            {
                // Log the error (use ILogger for production scenarios)
                Console.WriteLine($"Error fetching customers: {ex.Message}");
                return new List<CustomerModel>();
            }
        }


        public async Task<IActionResult> VehicleManagement(string searchQuery = "")
        {
            string businessId = BusinessID.businessId; // Replace with the logged-in user's business ID
            string managerId = BusinessID.userId; // Replace with the logged-in user's manager ID

            Console.WriteLine($"BusinessID: {businessId}, ManagerID: {managerId}");

            // Fetch vehicles asynchronously
            var vehicles = await GetVehiclesAsync(businessId, managerId);

            // Filter vehicles based on the search query (case-insensitive)
            if (!string.IsNullOrEmpty(searchQuery))
            {
                vehicles = vehicles.Where(v =>
                    v.vehicleNumPlate.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                    v.vehicleOwner.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                    v.vehicleMake.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                    v.vehicleModel.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            return View(vehicles);
        }

        private async Task<List<VehicleModel>> GetVehiclesAsync(string businessId, string managerId)
        {
            try
            {
                // Fetch vehicle details under the businessId
                var vehicles = await _firebaseClient
                    .Child($"Users/{businessId}/Vehicles")
                    .OnceAsync<VehicleModel>();

                Console.WriteLine($"Fetched {vehicles.Count} vehicles from Firebase.");

                var vehicleList = new List<VehicleModel>();

                foreach (var v in vehicles)
                {
                    // Fetch the front image URL for each vehicle
                    var frontImageUrls = await _firebaseClient
                        .Child($"Users/{businessId}/Vehicles/{v.Key}/images/front")
                        .OnceAsync<string>();

                    // Take the first URL or set to null if none exist
                    var frontImageURL = frontImageUrls.FirstOrDefault()?.Object;

                    vehicleList.Add(new VehicleModel
                    {
                        vehicleNumPlate = v.Object.vehicleNumPlate,
                        vehicleMake = v.Object.vehicleMake,
                        vehicleModel = v.Object.vehicleModel,
                        vehicleYear = v.Object.vehicleYear,
                        vehicleKms = v.Object.vehicleKms,
                        vinNumber = v.Object.vinNumber,
                        vehicleOwner = v.Object.vehicleOwner,
                        frontImageURL = frontImageURL
                    });
                }

                return vehicleList;
            }
            catch (Exception ex)
            {
                // Log the error (use ILogger for production scenarios)
                Console.WriteLine($"Error fetching vehicles: {ex.Message}");
                return new List<VehicleModel>();
            }
        }

     

        public async Task<IActionResult> InventoryManagement()
        {
            await fetchInventory();
            return View();
        }

        public async Task<IActionResult> EmployeeManagement(string searchQuery = "")
        {
            string businessId = BusinessID.businessId; // Replace with the logged-in user's business ID
            string managerId = BusinessID.userId; // Replace with logged-in user's manager ID

            Console.WriteLine($"BusinessID: {businessId}, ManagerID: {managerId}");

            var employees = await GetEmployeesAsync(businessId, managerId);

            if (!string.IsNullOrEmpty(searchQuery))
            {
                // Filter employees based on the search query (case-insensitive)
                employees = employees.Where(e =>
                    e.firstName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                    e.lastName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                    e.email.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                    e.phone.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }


            return View(employees);
        }

        private async Task<List<EmployeeModel>> GetEmployeesAsync(string businessId, string managerId)
        {
            try
            {
                // Fetch employees under the businessId
                var employees = await _firebaseClient
                    .Child($"Users/{businessId}/Employees")
                    .OnceAsync<EmployeeModel>();

                Console.WriteLine($"Fetched {employees.Count} employees from Firebase.");

                // Filter employees by managerId
                var filteredEmployees = employees
                    .Where(e => e.Object.managerID == managerId)
                    .Select(e => new EmployeeModel
                    {
                        EmployeeID = e.Key,
                        firstName = e.Object.firstName,
                        lastName = e.Object.lastName,
                        email = e.Object.email,
                        phone = e.Object.phone,
                        managerID = e.Object.managerID,
                        address = e.Object.address
                    })
                    .ToList();

                Console.WriteLine($"Filtered {filteredEmployees.Count} employees for ManagerID: {managerId}");
                return filteredEmployees;
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error fetching employees: {ex.Message}");
                return new List<EmployeeModel>();
            }
        }
        [HttpPost]
        public async Task<IActionResult> EditEmployee(EmployeeModel model, IFormFile ProfileImage)
        {
            string businessId = BusinessID.businessId;

            try
            {
                // Fetch existing employee data to avoid overwriting other properties
                var existingEmployee = await GetEmployeeByIdAsync(businessId, model.EmployeeID);
                if (existingEmployee == null)
                {
                    TempData["ErrorMessage"] = "Employee not found!";
                    return RedirectToAction("EmployeeManagement");
                }

                // Update properties from the form model
                existingEmployee.firstName = model.firstName;
                existingEmployee.lastName = model.lastName;
                existingEmployee.email = model.email;
                existingEmployee.phone = model.phone;
                existingEmployee.managerID = model.managerID;



                // Update the employee data in Firebase with the modified data
                await _firebaseClient
                    .Child($"Users/{businessId}/Employees/{model.EmployeeID}")
                    .PutAsync(existingEmployee);

                TempData["SuccessMessage"] = "Employee details updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating employee: {ex.Message}";
            }

            return RedirectToAction("EmployeeManagement");
        }

        private async Task<EmployeeModel> GetEmployeeByIdAsync(string businessId, string employeeId)
        {
            try
            {
                var employee = await _firebaseClient
                    .Child($"Users/{businessId}/Employees/{employeeId}")
                    .OnceSingleAsync<EmployeeModel>();

                return employee;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching employee: {ex.Message}");
                return null;
            }
        }

        public async Task<IActionResult> AnalyticsAsync()
        {
            await getServicesAnalytics();

            await getEmployeeAnalytics();

            return View();
        }

        public async Task<IActionResult> Services(string searchQuery = "")
{
    try
    {
        // Fetch all services
        var services = await FetchAllServicesAsync(BusinessID.businessId);

        // Filter services if a search query is provided
        if (!string.IsNullOrEmpty(searchQuery))
        {
            services = services.Where(service =>
                service.Customer.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                service.Model.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                service.NumberPlate.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                service.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        // Pass the services to the view using ViewBag
        ViewBag.Services = services;

        return View();
    }
    catch (Exception ex)
    {
        // Log and handle errors
        Console.WriteLine($"Error in Services method: {ex.Message}");
        ViewBag.ErrorMessage = "An error occurred while fetching services.";
        ViewBag.Services = null;
        return View();
    }
}


        //fetches analytics data for services
        private async Task getServicesAnalytics()
        {
            //counting completed services
            try
            {
                // Retrieve all services under the given path
                var services = await _firebaseClient
                    .Child($"Users/{businessId}/Services")
                    .OnceAsync<dynamic>();

                //total service count
                int serviceCount = services.Count();

                ViewBag.ServiceCount = serviceCount;

                // Count services with status set to "Completed"
                int completedServicesCount = services.Count(service =>
                    service.Object.status != null && service.Object.status.ToString() == "Completed");

                ViewBag.NumServicesCompleted = completedServicesCount;


                //counting busy services
                int busyServicesCount = services.Count(service =>
                    service.Object.status != null && service.Object.status.ToString() == "Busy");

                ViewBag.NumServicesBusy = busyServicesCount;

                //counting not started services
                int notStartedServicesCount = services.Count(service =>
                    service.Object.status != null && service.Object.status.ToString() == "Not Started");

                ViewBag.NumServicesNotStarted = notStartedServicesCount;

                //counting paid services
                int paidServicesCount = services.Count(service =>
                    Convert.ToBoolean(service.Object.paid.ToString()));

                ViewBag.NumServicesPaid = paidServicesCount;

                //counting unpaid services
                int unpaidServicesCount = services.Count(service =>
                    !Convert.ToBoolean(service.Object.paid.ToString()));

                ViewBag.NumServicesUnpaid = unpaidServicesCount;

                Console.WriteLine($"Paid: {paidServicesCount}, Unpaid: {unpaidServicesCount}");



            }
            catch (Exception e)
            {
                ViewBag.ErrorMessage = $"Error: {e.Message}";
            }

        }

        private async Task getEmployeeAnalytics()
        {
            try
            {
                var employees = await _firebaseClient
                    .Child($"Users/{businessId}/Employees")
                    .OnceAsync<dynamic>();

                //total service count
                int empCount = employees.Count();

                ViewBag.EmployeeCount = empCount;

                //counting different emplloyee types
                int empEmpCount = employees.Count(employee =>
                    employee.Object.role != null && employee.Object.role.ToString() == "employee");

                ViewBag.NumEmployeeEmployee = empEmpCount;


                int adminEmpCount = employees.Count(employee =>
                    employee.Object.role != null && employee.Object.role.ToString() == "admin");

                ViewBag.NumAdminEmployee = adminEmpCount;


                int ownerEmpCount = employees.Count(employee =>
                    employee.Object.role != null && employee.Object.role.ToString() == "owner");

                ViewBag.NumOwnerEmployee = ownerEmpCount;

                Console.WriteLine($"emps: {empEmpCount}, owners: {ownerEmpCount}, admins: {adminEmpCount}");
            }
            catch (Exception e)
            {
                ViewBag.ErrorMessage = $"Error: {e.Message}";
            }
        }

        private async Task<List<dynamic>> FetchAllServicesAsync(string businessId)
        {
            try
            {
                var services = await _firebaseClient.Child($"Users/{businessId}/Services").OnceAsync<dynamic>();

                var serviceList = new List<dynamic>();

                foreach (var service in services)
                {
                    string vehicleModel = await fetchVehicleModel((string)service.Object.vehicleID);
                    string vehicleNumberPlate = await fetchVehicleNumPlate((string)service.Object.vehicleID);
                    string custName = await fetchCustName((string)service.Object.custID);

                    string dateTakenIn = service.Object.dateReceived?.time != null
                        ? DateTimeOffset.FromUnixTimeMilliseconds((long)service.Object.dateReceived.time).ToString("dd MMMM yyyy")
                        : "N/A";

                    string dateReturned = service.Object.dateReturned?.time != null
                        ? DateTimeOffset.FromUnixTimeMilliseconds((long)service.Object.dateReturned.time).ToString("dd MMMM yyyy")
                        : "N/A";

                    string totalCost = $"R {service.Object.totalCost}";

                    serviceList.Add(new
                    {
                        Name = (string)service.Object.name,
                        Status = (string)service.Object.status,
                        Customer = custName,
                        Model = vehicleModel,
                        NumberPlate = vehicleNumberPlate,
                        DateTakenIn = dateTakenIn,
                        DateReturned = dateReturned,
                        TotalCost = totalCost
                    });
                }

                return serviceList;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error fetching services: {e.Message}");
                return new List<dynamic>();
            }
        }


        private async Task<string> fetchVehicleNumPlate(dynamic vehicleID)
        {
            try
            {
                return await _firebaseClient.Child($"Users/{BusinessID.businessId}/Vehicles/{vehicleID}/vehicleNumPlate").OnceSingleAsync<String>();
            }
            catch (Exception e)
            {
                return "Could not load vehicle data";
            }
        }

        private async Task<string> fetchVehicleModel(dynamic vehicleID)
        {
            string vehMakeModel = "Could not load vehicle data";

            try
            {
                string vehMake = await _firebaseClient.Child($"Users/{BusinessID.businessId}/Vehicles/{vehicleID}/vehicleMake").OnceSingleAsync<String>();
                string vehModel = await _firebaseClient.Child($"Users/{BusinessID.businessId}/Vehicles/{vehicleID}/vehicleModel").OnceSingleAsync<String>();

                vehMakeModel = vehMake + " " + vehModel;
            }
            catch (Exception e)
            {
                ViewBag.ErrorMessage = $"Error: {e.Message}";
            }

            return vehMakeModel;
        }

        private async Task<string> fetchCustName(dynamic custID)
        {
            string fullName = "Could not load customer data";

            try
            {
                string firstName = await _firebaseClient.Child($"Users/{BusinessID.businessId}/Customers/{custID}/CustomerName").OnceSingleAsync<String>();
                string surname = await _firebaseClient.Child($"Users/{BusinessID.businessId}/Customers/{custID}/CustomerSurname").OnceSingleAsync<String>();

                fullName = firstName + " " + surname;
            }
            catch (Exception e)
            {
                ViewBag.ErrorMessage = $"Error: {e.Message}";
            }

            return fullName;
        }

        private async Task fetchQuoteRequests()
        {
            try
            {
                // Retrieve all quote requests stored under the business ID
                var quoteRequestsResponse = await _firebaseClient
                    .Child($"Users/{BusinessID.businessId}/QuoteRequests")
                    .OnceAsync<QuoteRequestModel>();

                var quoteRequestsList = new List<dynamic>();

                // Iterate over the results and prepare the data for display
                foreach (var quoteRequest in quoteRequestsResponse)
                {
                    quoteRequestsList.Add(new
                    {
                        CustomerName = quoteRequest.Object.CustomerName,
                        CarMake = quoteRequest.Object.CarMake,
                        CarModel = quoteRequest.Object.CarModel,
                        Description = quoteRequest.Object.Description,
                        PhoneNumber = quoteRequest.Object.PhoneNumber,
                        RequestId = quoteRequest.Key // Firebase unique key
                    });
                }

                // Pass the data to the view
                ViewBag.quoteRequests = quoteRequestsList;
            }
            catch (Exception e)
            {
                // Handle and log errors
                ViewBag.ErrorMessage = $"Error: {e.Message}";
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteQuoteRequest(string requestId)
        {
            try
            {
                // Delete the quote request from Firebase
                await _firebaseClient
                    .Child($"Users/{BusinessID.businessId}/QuoteRequests/{requestId}")
                    .DeleteAsync();

                TempData["SuccessMessage"] = "Quote request marked as done.";
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = $"Error: {e.Message}";
            }

            // Refresh the list of quote requests
            await fetchQuoteRequests();
            return RedirectToAction("QuoteGenerator");
        }

        private async Task fetchInventory()
        {
            try
            {
                var inventoryResponse = await _firebaseClient.Child($"Users/{BusinessID.businessId}/parts").OnceAsync<dynamic>();

                var inventoryList = new List<dynamic>();

                foreach (var part in inventoryResponse)
                {
                    inventoryList.Add(new
                    {
                        partName = part.Object.partName,
                        costPrice = part.Object.costPrice,
                        stockCount = part.Object.stockCount
                    });
                }

                ViewBag.inventory = inventoryList;
            }
            catch (Exception e)
            {
                ViewBag.ErrorMessage = $"Error: {e.Message}";
            }
        }

        public IActionResult Logout()
        {
            // Log out the user
            Response.Cookies.Delete(".AspNetCore.Identity.Application");

            // Redirect to a confirmation page or homepage
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> SendPasswordResetEmail()
        {
            await _authProvider.SendPasswordResetEmailAsync(BusinessID.email);
            Console.WriteLine("Email sent successfully");
            return RedirectToAction("Account", "Admin");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccount()
        {
            try
            {
                string userId = BusinessID.userId;
                string businessId = BusinessID.businessId;

                // Delete customer data from Firebase
                await _firebaseClient.Child($"Users/{businessId}/Employees/{userId}").DeleteAsync();

                return Logout();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting account: {ex.Message}");
                TempData["ErrorMessage"] = "Unable to delete account. Please try again.";
                return RedirectToAction("Account", "Admin");
            }
        }

        public IActionResult Account()
        {
            return View();
        }






















































        #region Leave Management with Date Filtering

        [HttpGet]
        public async Task<IActionResult> LeaveManagement()
        {
            // Initialize the model with all approved leaves
            var model = new LeaveModel
            {
                Leaves = await GetAllApprovedLeavesAsync(businessId)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> FilterLeaves(string startDate, string endDate)
        {
            var model = new LeaveModel
            {
                StartDate = startDate,
                EndDate = endDate
            };

            Console.WriteLine($"Filtering leaves from {startDate} to {endDate}");

            // Try parsing the start and end dates
            if (TryParseDate(startDate, out var parsedStartDate) && TryParseDate(endDate, out var parsedEndDate))
            {
                // Filter leaves based on the date range
                model.Leaves = await GetLeavesWithinRangeAsync(businessId, parsedStartDate, parsedEndDate);

                // If leaves are found, log it, otherwise show message
                if (model.Leaves.Any())
                {
                    Console.WriteLine($"{model.Leaves.Count} leaves found in the selected range.");
                }
                else
                {
                    Console.WriteLine("No leaves found in the selected date range.");
                    TempData["InfoMessage"] = "No leave requests found within the selected date range.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid date format. Please provide valid start and end dates.";
            }

            return View("LeaveManagement", model);
        }

        // Fetch all approved leaves
        private async Task<List<LeaveDetail>> GetAllApprovedLeavesAsync(string businessId)
        {
            var employeeLeaves = new List<LeaveDetail>();

            try
            {
                var employees = await _firebaseClient
                    .Child($"Users/{businessId}/Employees")
                    .OnceAsync<Dictionary<string, object>>();

                Console.WriteLine($"Retrieved {employees.Count} employees from Firebase.");

                foreach (var employee in employees)
                {
                    Console.WriteLine($"Processing employee: {employee.Key}");
                    Console.WriteLine($"Employee Data: {JsonConvert.SerializeObject(employee.Object)}");

                    // Attempt to retrieve ApprovedLeave
                    if (employee.Object.TryGetValue("ApprovedLeave", out var approvedLeaveObj))
                    {
                        Console.WriteLine($"Found ApprovedLeave for employee {employee.Key}");

                        if (approvedLeaveObj is JObject approvedLeaveJObject)
                        {
                            // Deserialize JObject into a Dictionary
                            var approvedLeaves = approvedLeaveJObject.ToObject<Dictionary<string, object>>();
                            foreach (var leave in approvedLeaves.Values)
                            {
                                if (leave is JObject leaveDataJObject)
                                {
                                    var leaveData = leaveDataJObject.ToObject<Dictionary<string, string>>();
                                    if (leaveData != null &&
                                        leaveData.TryGetValue("startDate", out var startDate) &&
                                        leaveData.TryGetValue("endDate", out var endDate) &&
                                        leaveData.TryGetValue("userName", out var userName))
                                    {
                                        if (TryParseDate(endDate, out var parsedEndDate) && parsedEndDate >= DateTime.Now)
                                        {
                                            employeeLeaves.Add(new LeaveDetail
                                            {
                                                EmployeeName = userName,
                                                StartDate = startDate,
                                                EndDate = endDate
                                            });
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"ApprovedLeave data could not be parsed for employee {employee.Key}: {approvedLeaveObj.GetType()}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Employee {employee.Key} has no ApprovedLeave data.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving approved leaves: {ex.Message}");
            }

            return employeeLeaves;
        }


        // Parse date with multiple formats
        private bool TryParseDate(string date, out DateTime parsedDate)
        {
            var formats = new[] { "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd" }; // Add more formats if necessary
            return DateTime.TryParseExact(date, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);
        }

        // Fetch leaves within the specified date range
        private async Task<List<LeaveDetail>> GetLeavesWithinRangeAsync(string businessId, DateTime startDate, DateTime endDate)
        {
            Console.WriteLine($"Fetching leaves within range: {startDate:dd/MM/yyyy} to {endDate:dd/MM/yyyy}");

            // Retrieve all leaves
            var allLeaves = await GetAllApprovedLeavesAsync(businessId);

            if (!allLeaves.Any())
            {
                Console.WriteLine("No leaves were retrieved.");
                return new List<LeaveDetail>();
            }

            Console.WriteLine($"{allLeaves.Count} total leaves retrieved.");

            // Filter leaves based on the date range
            var filteredLeaves = allLeaves.Where(leave =>
                TryParseDate(leave.StartDate, out var leaveStartDate) &&
                TryParseDate(leave.EndDate, out var leaveEndDate) &&
                leaveStartDate <= endDate && leaveEndDate >= startDate) // Leave must overlap with the range
                .ToList();

            Console.WriteLine($"{filteredLeaves.Count} leaves found after filtering.");

            return filteredLeaves;
        }

        #endregion
    }


}
