using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class Flight
{
    public string FlightNumber { get; set; }
    public string Airline { get; set; }
    public string Destination { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public FlightStatus Status { get; set; }
    public TimeSpan Duration { get; set; }
    public string AircraftType { get; set; }
    public string Terminal { get; set; }
}

public enum FlightStatus
{
    OnTime,
    Delayed,
    Cancelled,
    Boarding,
    InFlight
}

public class FlightInfoSystem
{
    private List<Flight> flights = new List<Flight>();

    public void AddFlight(Flight flight) => flights.Add(flight);

    public void RemoveFlight(string flightNumber)
    {
        flights.RemoveAll(f => f.FlightNumber == flightNumber);
    }
    public class FlightData
    {
        [JsonProperty("flights")]
        public List<Flight> Flights { get; set; }
    }
    public List<Flight> SearchFlightsByAirline(string airline)
    {
        var result = flights.FindAll(f => f.Airline == airline);
        result.Sort((f1, f2) => f1.DepartureTime.CompareTo(f2.DepartureTime));
        return result;
    }

    public List<Flight> SearchDelayedFlights()
    {
        var result = flights.FindAll(f => f.Status == FlightStatus.Delayed);
        result.Sort((f1, f2) => f1.DepartureTime.CompareTo(f2.DepartureTime));
        return result;
    }

    public List<Flight> SearchFlightsByDepartureDate(DateTime departureDate)
    {
        var result = flights.FindAll(f => f.DepartureTime.Date == departureDate.Date);
        result.Sort((f1, f2) => f1.DepartureTime.CompareTo(f2.DepartureTime));
        return result;
    }

    public List<Flight> SearchFlightsByTimeAndDestination(DateTime startTime, DateTime endTime, string destination)
    {
        var result = flights.FindAll(f =>
            f.DepartureTime >= startTime && f.DepartureTime <= endTime && f.Destination == destination);
        result.Sort((f1, f2) => f1.DepartureTime.CompareTo(f2.DepartureTime));
        return result;
    }

    public List<Flight> SearchRecentArrivals(DateTime endTime)
    {
        DateTime startTime = endTime.AddHours(-1); // One hour ago
        var result = flights.FindAll(f => f.ArrivalTime >= startTime && f.ArrivalTime <= endTime);
        result.Sort((f1, f2) => f1.ArrivalTime.CompareTo(f2.ArrivalTime));
        return result;
    }

    public void LoadFlightsFromJson(string filePath)
    {
        try
        {
            string jsonData = File.ReadAllText(filePath);
            var flightData = JsonConvert.DeserializeObject<FlightData>(jsonData);

            if (flightData?.Flights != null)
            {
                flights = flightData.Flights;
                Console.WriteLine("Flights loaded successfully.");
            }
            else
            {
                Console.WriteLine("Error: No valid flight data found in the JSON file.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading flights from JSON: {ex.Message}");
        }
    }

    public string SerializeFlightsToJson()
    {
        try
        {
            return JsonConvert.SerializeObject(flights, Newtonsoft.Json.Formatting.Indented);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error serializing flights to JSON: {ex.Message}");
            return null;
        }
    }
}


class Program
{
    static void Main()
    {
        var flightSystem = InitializeFlightSystem();

        RemoveFlightExample(flightSystem);

        var newFlight = CreateNewFlight();
        AddNewFlightExample(flightSystem, newFlight);

        Console.WriteLine("Flights by airline:");
        PrintFlights(flightSystem.SearchFlightsByAirline("MAU"));

        Console.WriteLine("\nDelayed flights:");
        PrintFlights(flightSystem.SearchDelayedFlights());

        Console.WriteLine("\nFlights on a specific date:");
        PrintFlightsOnSpecificDate(flightSystem, DateTime.Now.Date);

        Console.WriteLine("\nFlights in a specific time range to a specific destination:");
        PrintFlightsInTimeAndDestinationRange(flightSystem, DateTime.Now, DateTime.Now.AddHours(6), "London");

        Console.WriteLine("\nRecent arrivals:");
        PrintRecentArrivals(flightSystem, DateTime.Now);

        SaveFlightsToFile(flightSystem, "flights_data_updated.json");
    }

    static FlightInfoSystem InitializeFlightSystem()
    {
        var flightSystem = new FlightInfoSystem();
        flightSystem.LoadFlightsFromJson("flights_data.json");
        return flightSystem;
    }

    static void RemoveFlightExample(FlightInfoSystem flightSystem)
    {
        flightSystem.RemoveFlight("XYZ789");
    }

    static Flight CreateNewFlight()
    {
        return new Flight
        {
            FlightNumber = "AB123",
            Airline = "MAU",
            Destination = "Donbass",
            DepartureTime = new DateTime(2023, 12, 27, 15, 30, 0),
            ArrivalTime = new DateTime(2023, 12, 27, 17, 30, 0),
            Status = FlightStatus.OnTime,
            Duration = TimeSpan.FromHours(2)
        };
    }

    static void AddNewFlightExample(FlightInfoSystem flightSystem, Flight newFlight)
    {
        flightSystem.AddFlight(newFlight);
    }

    static void PrintFlights(List<Flight> flights)
    {
        foreach (var flight in flights)
        {
            Console.WriteLine($"{flight.FlightNumber} - {flight.Airline} - {flight.Destination} - " +
                              $"{flight.DepartureTime} - {flight.ArrivalTime} - {flight.Status}-{flight.AircraftType}-{flight.Terminal}");
        }
    }

    static void PrintFlightsOnSpecificDate(FlightInfoSystem flightSystem, DateTime date)
    {
        var specificDateFlights = flightSystem.SearchFlightsByDepartureDate(date);
        PrintFlights(specificDateFlights);
    }

    static void PrintFlightsInTimeAndDestinationRange(FlightInfoSystem flightSystem, DateTime startTime, DateTime endTime, string destination)
    {
        var timeAndDestinationFlights = flightSystem.SearchFlightsByTimeAndDestination(startTime, endTime, destination);
        PrintFlights(timeAndDestinationFlights);
    }

    static void PrintRecentArrivals(FlightInfoSystem flightSystem, DateTime referenceTime)
    {
        var recentArrivals = flightSystem.SearchRecentArrivals(referenceTime);
        PrintFlights(recentArrivals);
    }

    static void SaveFlightsToFile(FlightInfoSystem flightSystem, string fileName)
    {
        var serializedData = flightSystem.SerializeFlightsToJson();
        File.WriteAllText(fileName, serializedData);
    }
}

