@bdd @workflow
Feature: Calculator service workflows
  As a laboratory assistant
  I want to process one request interactively or many requests in a batch
  So that the calculator can support both manual work and repeatable file-based runs

  Background:
    Given the calculator service is ready

  Rule: Each processed request keeps its identifier in the reported output
    Scenario: User enters one request without exporting the result
      Given console mode is selected for the calculator service
      And a console request with id "manual-1" for operation "add" and method "" using operands "10" and "20"
      When I run the calculator service
      Then the last shown result should be "30"
      And the last shown result id should be "manual-1"
      But no results should be written to a file

  Rule: Batch processing preserves request order and writes one result per input row
    Scenario: File mode processes a table of operations
      Given file mode is selected with input path "input.json"
      And the output path is "results.json"
      And the input operations are
        | Id  | Operation | Method  | Operand1 | Operand2 |
        | op1 | add       |         | 5        | 5        |
        | op2 | multiply  | classic | 3        | 4        |
        | op3 | divide    | binary  | 23       | 5        |
      When I run the calculator service
      Then the saved results should be
        | Id  | Result |
        | op1 | 10     |
        | op2 | 12     |
        | op3 | 4      |
      And the output should be written to "results.json"
