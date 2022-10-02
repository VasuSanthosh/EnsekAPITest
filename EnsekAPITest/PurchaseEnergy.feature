Feature: PurchaseEnergy

Scenario to purchase energy and verify the details
@Scenario
Scenario Outline: Verify the availablity of the energy , purchase them and Check for Feb orders
	Given the user checks the status of the <EnergyType>
	When the user try to purchase the <EnergyTypeId> and <Quantity>
	Then the response should contain a valid Message <Quantity> <UnitType> <CostPerUnit> <statuscode>  and units remaining
	And the order details <EnergyType>,<Quantity> are saved to the system
	And the feb orders are verified
	Examples: 
	| EnergyType | EnergyTypeId | Quantity | UnitType | CostPerUnit | statuscode |
	| gas        | 1            | 1        | m³       | 0.34        | 200        |
	| nuclear    | 2            | 1        | MW       | 0.56        | 200        |
	| electric   | 3            | 1        | kWh      | 0.47        | 200        |
	| oil        | 4            | 1        | Litres   | 0.5         | 200        |
	