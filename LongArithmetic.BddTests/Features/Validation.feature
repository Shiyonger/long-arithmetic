@bdd @validation @declarative
Feature: Validation of incoming calculator requests
  As a calculator operator
  I want invalid requests to be rejected before computation starts
  So that the service only returns meaningful long arithmetic results

  Background:
    Given console mode is selected for the calculator service

  Rule: Only non-empty positive integer operands are accepted
    Scenario: Request with a negative operand is rejected
      Given a console request with id "neg-1" for operation "add" and method "" using operands "-5" and "3"
      When I run the calculator service
      Then the last shown result should be "Некорректные данные"
      And no results should be written to a file

    Scenario: Request with alphabetic data is rejected
      Given a console request with id "alpha-1" for operation "subtract" and method "" using operands "abc" and "1"
      When I run the calculator service
      Then the last shown result should be "Некорректные данные"
      But the service should still report the original request identifier "alpha-1"
