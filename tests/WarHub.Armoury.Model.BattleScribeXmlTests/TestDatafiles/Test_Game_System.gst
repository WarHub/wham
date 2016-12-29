<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<gameSystem id="7e2a1cff-7498-69af-6b1e-04c38234a3c5" name="Test Game System" book="book-placeholder" revision="1" battleScribeVersion="2.00" authorName="Author-Name" authorContact="Author-Contact" authorUrl="http://www.Author-URL.com" xmlns="http://www.battlescribe.net/schema/gameSystemSchema">
  <profiles/>
  <rules/>
  <infoLinks/>
  <costTypes>
    <costType id="points" name="pts" defaultCostLimit="0.0"/>
  </costTypes>
  <profileTypes>
    <profileType id="7e6737a8-902d-b4b2-4cf7-55d4dfc723ba" name="Test Profile Type">
      <characteristicTypes>
        <characteristicType id="cc05a167-9e9a-6ae2-01da-633ea6aedde2" name="New Characteristic"/>
      </characteristicTypes>
    </profileType>
  </profileTypes>
  <forceEntries>
    <forceEntry id="cad557e3-279d-b9af-3fe0-5b92360fd0ab" name="Test Force Type" hidden="false">
      <profiles/>
      <rules/>
      <infoLinks/>
      <modifiers/>
      <constraints/>
      <categoryEntries>
        <categoryEntry id="896d8ba8-08ac-d2da-782d-a037ef086789" name="Modifier Test" hidden="false">
          <profiles/>
          <rules/>
          <infoLinks/>
          <modifiers>
            <modifier type="set" field="minPoints" value="0.0">
              <repeats/>
              <conditions/>
              <conditionGroups/>
            </modifier>
            <modifier type="increment" field="maxPoints" value="0.0">
              <repeats/>
              <conditions/>
              <conditionGroups/>
            </modifier>
            <modifier type="decrement" field="maxSelections" value="0.0">
              <repeats/>
              <conditions/>
              <conditionGroups/>
            </modifier>
            <modifier type="decrement" field="minPercentage" value="0.0">
              <repeats/>
              <conditions/>
              <conditionGroups/>
            </modifier>
            <modifier type="increment" field="minSelections" value="0.0">
              <repeats/>
              <conditions/>
              <conditionGroups/>
            </modifier>
            <modifier type="append" field="name" value="0.0">
              <repeats/>
              <conditions/>
              <conditionGroups/>
            </modifier>
          </modifiers>
          <constraints>
            <constraint field="selections" scope="parent" value="0.0" percentValue="false" shared="false" includeChildSelections="false" includeChildForces="false" id="minSelections" type="min"/>
            <constraint field="selections" scope="parent" value="-1.0" percentValue="false" shared="false" includeChildSelections="false" includeChildForces="true" id="maxSelections" type="max"/>
            <constraint field="points" scope="parent" value="0.0" percentValue="false" shared="false" includeChildSelections="true" includeChildForces="false" id="minPoints" type="min"/>
            <constraint field="points" scope="parent" value="-1.0" percentValue="false" shared="false" includeChildSelections="true" includeChildForces="true" id="maxPoints" type="max"/>
            <constraint field="limit::points" scope="roster" value="0.0" percentValue="true" shared="false" includeChildSelections="true" includeChildForces="false" id="minPercentage" type="min"/>
          </constraints>
        </categoryEntry>
        <categoryEntry id="cca20a2e-516b-5eb7-f5f3-7685495497dc" name="Repeat modifier test" hidden="false">
          <profiles/>
          <rules/>
          <infoLinks/>
          <modifiers>
            <modifier type="set" field="minSelections" value="0.0">
              <repeats>
                <repeat field="selections" scope="force" value="1.0" percentValue="false" shared="false" includeChildSelections="false" includeChildForces="false" childId="896d8ba8-08ac-d2da-782d-a037ef086789" repeats="1"/>
              </repeats>
              <conditions/>
              <conditionGroups/>
            </modifier>
            <modifier type="set" field="minSelections" value="0.0">
              <repeats>
                <repeat field="points" scope="force" value="1.0" percentValue="true" shared="false" includeChildSelections="true" includeChildForces="false" childId="896d8ba8-08ac-d2da-782d-a037ef086789" repeats="1"/>
              </repeats>
              <conditions/>
              <conditionGroups/>
            </modifier>
            <modifier type="set" field="minSelections" value="0.0">
              <repeats>
                <repeat field="selections" scope="force" value="1.0" percentValue="false" shared="false" includeChildSelections="false" includeChildForces="false" childId="any" repeats="1"/>
              </repeats>
              <conditions/>
              <conditionGroups/>
            </modifier>
            <modifier type="set" field="minSelections" value="0.0">
              <repeats>
                <repeat field="points" scope="force" value="1.0" percentValue="false" shared="true" includeChildSelections="false" includeChildForces="false" childId="any" repeats="1"/>
              </repeats>
              <conditions/>
              <conditionGroups/>
            </modifier>
            <modifier type="set" field="minSelections" value="0.0">
              <repeats>
                <repeat field="points" scope="force" value="1.0" percentValue="false" shared="false" includeChildSelections="false" includeChildForces="false" childId="896d8ba8-08ac-d2da-782d-a037ef086789" repeats="1"/>
              </repeats>
              <conditions/>
              <conditionGroups/>
            </modifier>
          </modifiers>
          <constraints>
            <constraint field="selections" scope="parent" value="0.0" percentValue="false" shared="false" includeChildSelections="false" includeChildForces="false" id="minSelections" type="min"/>
          </constraints>
        </categoryEntry>
        <categoryEntry id="d99ab4b1-4076-e79a-c3f3-287fcd813394" name="Category Modifier Conditions" hidden="false">
          <profiles/>
          <rules/>
          <infoLinks/>
          <modifiers>
            <modifier type="increment" field="minSelections" value="0.0">
              <repeats/>
              <conditions>
                <condition field="limit::points" scope="roster" value="0.0" percentValue="false" shared="false" includeChildSelections="false" includeChildForces="false" childId="any" type="lessThan"/>
                <condition field="limit::points" scope="roster" value="0.0" percentValue="false" shared="false" includeChildSelections="false" includeChildForces="false" childId="any" type="greaterThan"/>
                <condition field="limit::points" scope="roster" value="0.0" percentValue="false" shared="false" includeChildSelections="true" includeChildForces="true" childId="any" type="lessThan"/>
                <condition field="limit::points" scope="roster" value="0.0" percentValue="false" shared="false" includeChildSelections="false" includeChildForces="false" childId="any" type="notEqualTo"/>
                <condition field="limit::points" scope="roster" value="0.0" percentValue="false" shared="false" includeChildSelections="false" includeChildForces="false" childId="any" type="equalTo"/>
                <condition field="limit::points" scope="roster" value="0.0" percentValue="false" shared="false" includeChildSelections="false" includeChildForces="false" childId="any" type="atMost"/>
                <condition field="limit::points" scope="roster" value="0.0" percentValue="false" shared="false" includeChildSelections="false" includeChildForces="false" childId="any" type="atLeast"/>
                <condition field="selections" scope="roster" value="0.0" percentValue="false" shared="false" includeChildSelections="false" includeChildForces="false" childId="any" type="equalTo"/>
                <condition field="points" scope="parent" value="0.0" percentValue="true" shared="false" includeChildSelections="true" includeChildForces="false" childId="d99ab4b1-4076-e79a-c3f3-287fcd813394" type="equalTo"/>
                <condition field="points" scope="parent" value="0.0" percentValue="false" shared="false" includeChildSelections="false" includeChildForces="false" childId="d99ab4b1-4076-e79a-c3f3-287fcd813394" type="equalTo"/>
                <condition field="selections" scope="parent" value="0.0" percentValue="false" shared="false" includeChildSelections="false" includeChildForces="false" childId="d99ab4b1-4076-e79a-c3f3-287fcd813394" type="equalTo"/>
              </conditions>
              <conditionGroups/>
            </modifier>
            <modifier type="set" field="minSelections" value="0.0">
              <repeats/>
              <conditions/>
              <conditionGroups>
                <conditionGroup type="and">
                  <conditions>
                    <condition field="limit::points" scope="roster" value="0.0" percentValue="false" shared="false" includeChildSelections="false" includeChildForces="false" childId="any" type="equalTo"/>
                  </conditions>
                  <conditionGroups>
                    <conditionGroup type="or">
                      <conditions/>
                      <conditionGroups/>
                    </conditionGroup>
                  </conditionGroups>
                </conditionGroup>
              </conditionGroups>
            </modifier>
          </modifiers>
          <constraints>
            <constraint field="selections" scope="parent" value="0.0" percentValue="false" shared="false" includeChildSelections="false" includeChildForces="false" id="minSelections" type="min"/>
          </constraints>
        </categoryEntry>
      </categoryEntries>
      <forceEntries>
        <forceEntry id="cee5ca6c-940f-ec10-0584-39346e83c8f6" name="Child Force Type" hidden="false">
          <profiles/>
          <rules/>
          <infoLinks/>
          <modifiers/>
          <constraints>
            <constraint field="selections" scope="parent" value="1.0" percentValue="false" shared="false" includeChildSelections="false" includeChildForces="false" id="minSelections" type="min"/>
            <constraint field="points" scope="parent" value="1.0" percentValue="false" shared="false" includeChildSelections="true" includeChildForces="false" id="minPoints" type="min"/>
            <constraint field="limit::points" scope="roster" value="1.0" percentValue="true" shared="false" includeChildSelections="true" includeChildForces="false" id="minPercentage" type="min"/>
          </constraints>
          <categoryEntries>
            <categoryEntry id="09bf771c-870a-a78e-7acc-5914b3e9ba1c" name="ChildCategory" hidden="false">
              <profiles/>
              <rules/>
              <infoLinks/>
              <modifiers/>
              <constraints/>
            </categoryEntry>
          </categoryEntries>
          <forceEntries/>
        </forceEntry>
      </forceEntries>
    </forceEntry>
  </forceEntries>
  <selectionEntries/>
  <entryLinks/>
  <sharedSelectionEntries/>
  <sharedSelectionEntryGroups/>
  <sharedRules/>
  <sharedProfiles/>
</gameSystem>