namespace Converter
{

    class Program
    {
        static void Main(string[] args)
        {
            bool continueConversion = true;

            while (continueConversion)
            {
                string[] unitSystems = { "Imperial", "Metric" };
                string[] unitTypes = { "Length", "Mass", "Capacity" };
                string selectedSystem = "";
                string selectedType = "";
                string selectedUnit = "";
                double amount = 0;
                bool isValidAmount = false;

                // Unit System Selection
                do
                {
                    selectedSystem = Selector("Select the unit system:", unitSystems, includeGoBack: false);
                    Console.Clear(); // Clear the console after selecting the unit system

                    // Unit Type Selection
                    selectedType = Selector("Select the type of units:", unitTypes, includeGoBack: true);
                    if (selectedType == "Go Back")
                    {
                        continue; // Go back to unit system selection
                    }
                    Console.Clear(); // Clear the console after selecting the type of units

                    // Unit Selection
                    string[] units = GetUnitsByTypeAndSystem(selectedType, selectedSystem);
                    selectedUnit = Selector("Select the unit:", units, includeGoBack: true);
                    if (selectedUnit == "Go Back")
                    {
                        continue; // Go back to unit type selection
                    }
                    Console.Clear(); // Clear the console after selecting the unit

                    // Amount Input
                    do
                    {
                        Console.Write($"Enter the amount of {selectedUnit} you have: ");
                        string? input = Console.ReadLine();
                        isValidAmount = double.TryParse(input, out amount);
                        if (!isValidAmount)
                        {
                            Console.WriteLine("Invalid input. Please enter a valid number.");
                        }
                    } while (!isValidAmount);

                    break; // Proceed if all selections are made without going back
                } while (true); // Loop until a valid unit is selected

                Console.Clear(); // Clear the console before showing the final selection
                Console.WriteLine($"You selected: {amount} {selectedUnit}.");
                Thread.Sleep(1500);
                Console.Clear();

                // Convert the selected amount to a base unit
                double baseAmount = ConvertToBaseUnit(amount, selectedUnit, selectedType, selectedSystem);

                string targetSystem = "";
                string targetUnit = "";
                do
                {
                    targetSystem = Selector("Select the target unit system:", unitSystems);
                    Console.Clear(); // Clear the console after selecting the target unit system


                    // Unit Selection
                    string[] units = GetUnitsByTypeAndSystem(selectedType, targetSystem);
                    targetUnit = Selector("Select the target unit:", units, includeGoBack: true);
                    if (targetUnit == "Go Back")
                    {
                        continue; // Go back to target unit type selection
                    }
                    Console.Clear(); // Clear the console after selecting the target unit               

                    // amount input not needed for target unit

                    break; // Proceed if all selections are made without going back
                } while (true); // Loop until a valid target unit is selected

                // Convert from base unit to target unit
                double convertedAmount = ConvertFromBaseUnit(baseAmount, targetUnit, GetConversionRates(selectedType, targetSystem));

                Console.Clear(); // clear the console before showing the final conversion
                Console.WriteLine($"{amount} {selectedUnit} is equivalent to {Math.Round(convertedAmount, 2)} {targetUnit}.");
                Thread.Sleep(2000);
                // ask if they want to perform another conversion
                continueConversion = AskToContinue();
                Console.Clear();
            }
            Console.WriteLine("Thank you for using the converter. Goodbye!");
        }

        static bool AskToContinue()
        {
            string response = Selector("Do you want to perform another conversion?", new string[] { "Yes", "No" });
            return response == "Yes";
        }

        static string Selector(string prompt, string[] options, bool includeGoBack = false)
        {
            int selectedIndex = 0;
            ConsoleKey keyPressed;
            if (includeGoBack)
            {
                List<string> tempList = options.ToList();
                tempList.Add("Go Back");
                options = tempList.ToArray();
            }
            do
            {
                Console.Clear();
                Console.WriteLine(prompt);
                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"> {options[i]}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"  {options[i]}");
                    }
                }

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                keyPressed = keyInfo.Key;

                if (keyPressed == ConsoleKey.UpArrow)
                {
                    selectedIndex--;
                    if (selectedIndex < 0)
                    {
                        selectedIndex = options.Length - 1;
                    }
                }
                else if (keyPressed == ConsoleKey.DownArrow)
                {
                    selectedIndex++;
                    if (selectedIndex >= options.Length)
                    {
                        selectedIndex = 0;
                    }
                }
            } while (keyPressed != ConsoleKey.Enter);

            return options[selectedIndex];
        }

        static string[] GetUnitsByTypeAndSystem(string selectedType, string selectedSystem)
        {
            // Optimized to use a dictionary lookup
            var unitsByTypeAndSystem = new Dictionary<string, Dictionary<string, string[]>>
            {
                ["Length"] = new Dictionary<string, string[]>
                {
                    ["Imperial"] = new[] { "Inch", "Foot", "Yard", "Mile" },
                    ["Metric"] = new[] { "Millimeter", "Centimeter", "Meter", "Kilometer" }
                },
                ["Mass"] = new Dictionary<string, string[]>
                {
                    ["Imperial"] = new[] { "Ounce", "Pound", "Stone", "Ton" },
                    ["Metric"] = new[] { "Gram", "Kilogram", "Metric Ton" }
                },
                ["Capacity"] = new Dictionary<string, string[]>
                {
                    ["Imperial"] = new[] { "Teaspoon", "Tablespoon", "Cup", "Pint", "Quart", "Gallon" },
                    ["Metric"] = new[] { "Milliliter", "Centiliter", "Liter" }
                }
            };

            if (unitsByTypeAndSystem.TryGetValue(selectedType, out var systemDict) && systemDict.TryGetValue(selectedSystem, out var units))
            {
                return units;
            }
            return Array.Empty<string>();
        }

        static double ConvertToBaseUnit(double amount, string unit, string unitType, string unitSystem)
        {
            // Always convert to the Metric system as the base for simplicity
            var conversionRates = GetConversionRates(unitType, "Metric");
            if (conversionRates.ContainsKey(unit))
            {
                return amount * conversionRates[unit];
            }
            else
            {
                Console.WriteLine("Conversion rate for the selected unit not found.");
                return amount; // Return the original amount if conversion rate is not found
            }
        }

        static double ConvertFromBaseUnit(double baseAmount, string targetUnit, Dictionary<string, double> conversionRates)
        {
            // Conversion rates should be provided based on the target unit's system
            if (conversionRates.ContainsKey(targetUnit))
            {
                return baseAmount / conversionRates[targetUnit];
            }
            else
            {
                Console.WriteLine("Conversion rate for the target unit not found.");
                return baseAmount; // Return the original base amount if conversion rate is not found
            }
        }

        static Dictionary<string, double> GetConversionRates(string unitType, string unitSystem)
        {
            // This method now always returns conversion rates to/from the Metric system as the base
            switch (unitType)
            {
                case "Length":
                    return new Dictionary<string, double>
                    {
                        { "Millimeter", 0.001 },
                        { "Centimeter", 0.01 },
                        { "Meter", 1 },
                        { "Kilometer", 1000 },
                        { "Inch", 0.0254 },
                        { "Foot", 0.3048 },
                        { "Yard", 0.9144 },
                        { "Mile", 1609.34 }
                    };
                case "Mass":
                    return new Dictionary<string, double>
                    {
                        { "Gram", 1 },
                        { "Kilogram", 1000 },
                        { "Metric Ton", 1000000 },
                        { "Ounce", 28.3495 },
                        { "Pound", 453.592 },
                        { "Stone", 6350.29 },
                        { "Ton", 907185 }
                    };
                case "Capacity":
                    return new Dictionary<string, double>
                    {
                        { "Milliliter", 0.001 },
                        { "Centiliter", 0.01 },
                        { "Liter", 1 },
                        { "Teaspoon", 4.92892 },
                        { "Tablespoon", 14.7868 },
                        { "Cup", 236.588 },
                        { "Pint", 473.176 },
                        { "Quart", 946.353 },
                        { "Gallon", 3785.41 }
                    };
            }
            return new Dictionary<string, double>(); // Fallback empty dictionary
        }
    }
}
