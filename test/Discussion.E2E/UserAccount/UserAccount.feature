
Feature: Register and Signin
  As a math idiot
  I want to register a membership and signin
  So that I can identity myself 

  Background:
    Given I am on the dotnet club homepage

  Scenario: Register a new user
    Given I am on the dotnet club register page
    When I use as "hello" as username for registering
    And I use as "password1" as my password to register
    Then I am able to register
    And I am signed in and my username shows up on page

  Scenario Outline: Register a new user using invalid information
    Given I am on the dotnet club register page
    When I use as "<username>" as username for registering
    And I use as "<password>" as my password to register
    Then I am see an error
    And I am not signed in

    Examples: Animals
      | username   | password    |
      | panda    | Panda Express |
      | elephant | Elephant Man  |


  Scenario: Signin
    Given I am on the dotnet club signin page
    When I use as "hello" as username for signin
    And I use as "password1" as my password to signin
    Then I am able to signin
    And I am signed in and my username shows up on page

