<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<gameSystem id="7e2a1cff-7498-69af-6b1e-04c38234a3c5" revision="1" battleScribeVersion="1.15" name="Test Game System" books="book-placeholder" authorName="Author-Name" authorContact="Author-Contact" authorUrl="http://www.Author-URL.com" xmlns="http://www.battlescribe.net/schema/gameSystemSchema">
  <forceTypes>
    <forceType id="cad557e3-279d-b9af-3fe0-5b92360fd0ab" name="Test Force Type" minSelections="0" maxSelections="-1" minPoints="0.0" maxPoints="-1.0" minPercentage="0" maxPercentage="-1" countTowardsParentMinSelections="false" countTowardsParentMaxSelections="false" countTowardsParentMinPoints="false" countTowardsParentMaxPoints="false" countTowardsParentMinPercentage="false" countTowardsParentMaxPercentage="false">
      <categories>
        <category id="896d8ba8-08ac-d2da-782d-a037ef086789" name="Modifier Test" minSelections="0" maxSelections="-1" minPoints="0.0" maxPoints="-1.0" minPercentage="0" maxPercentage="-1" countTowardsParentMinSelections="false" countTowardsParentMaxSelections="false" countTowardsParentMinPoints="false" countTowardsParentMaxPoints="false" countTowardsParentMinPercentage="false" countTowardsParentMaxPercentage="false">
          <modifiers>
            <modifier type="set" field="minPoints" value="0.0" repeat="false" numRepeats="1" incrementParentId="roster" incrementField="points limit" incrementValue="1.0">
              <conditions/>
              <conditionGroups/>
            </modifier>
            <modifier type="increment" field="maxPoints" value="0.0" repeat="false" numRepeats="1" incrementParentId="roster" incrementField="points limit" incrementValue="1.0">
              <conditions/>
              <conditionGroups/>
            </modifier>
            <modifier type="decrement" field="maxSelections" value="0.0" repeat="false" numRepeats="1" incrementParentId="roster" incrementField="points limit" incrementValue="1.0">
              <conditions/>
              <conditionGroups/>
            </modifier>
            <modifier type="decrement" field="minPercentage" value="0.0" repeat="false" numRepeats="1" incrementParentId="roster" incrementField="points limit" incrementValue="1.0">
              <conditions/>
              <conditionGroups/>
            </modifier>
            <modifier type="increment" field="minSelections" value="0.0" repeat="false" numRepeats="1" incrementParentId="roster" incrementField="points limit" incrementValue="1.0">
              <conditions/>
              <conditionGroups/>
            </modifier>
            <modifier type="set" field="maxPercentage" value="0.0" repeat="false" numRepeats="1" incrementParentId="roster" incrementField="points limit" incrementValue="1.0">
              <conditions/>
              <conditionGroups/>
            </modifier>
          </modifiers>
        </category>
        <category id="cca20a2e-516b-5eb7-f5f3-7685495497dc" name="Repeat modifier test" minSelections="0" maxSelections="-1" minPoints="0.0" maxPoints="-1.0" minPercentage="0" maxPercentage="-1" countTowardsParentMinSelections="false" countTowardsParentMaxSelections="false" countTowardsParentMinPoints="false" countTowardsParentMaxPoints="false" countTowardsParentMinPercentage="false" countTowardsParentMaxPercentage="false">
          <modifiers>
            <modifier type="set" field="minSelections" value="0.0" repeat="true" numRepeats="1" incrementParentId="896d8ba8-08ac-d2da-782d-a037ef086789" incrementField="selections" incrementValue="1.0">
              <conditions/>
              <conditionGroups/>
            </modifier>
            <modifier type="set" field="minSelections" value="0.0" repeat="true" numRepeats="1" incrementParentId="896d8ba8-08ac-d2da-782d-a037ef086789" incrementField="percent" incrementValue="1.0">
              <conditions/>
              <conditionGroups/>
            </modifier>
            <modifier type="set" field="minSelections" value="0.0" repeat="true" numRepeats="1" incrementParentId="roster" incrementField="total selections" incrementValue="1.0">
              <conditions/>
              <conditionGroups/>
            </modifier>
            <modifier type="set" field="minSelections" value="0.0" repeat="true" numRepeats="1" incrementParentId="roster" incrementField="points limit" incrementValue="1.0">
              <conditions/>
              <conditionGroups/>
            </modifier>
            <modifier type="set" field="minSelections" value="0.0" repeat="true" numRepeats="1" incrementParentId="896d8ba8-08ac-d2da-782d-a037ef086789" incrementField="points" incrementValue="1.0">
              <conditions/>
              <conditionGroups/>
            </modifier>
          </modifiers>
        </category>
        <category id="d99ab4b1-4076-e79a-c3f3-287fcd813394" name="Category Modifier Conditions" minSelections="0" maxSelections="-1" minPoints="0.0" maxPoints="-1.0" minPercentage="0" maxPercentage="-1" countTowardsParentMinSelections="false" countTowardsParentMaxSelections="false" countTowardsParentMinPoints="false" countTowardsParentMaxPoints="false" countTowardsParentMinPercentage="false" countTowardsParentMaxPercentage="false">
          <modifiers>
            <modifier type="increment" field="minSelections" value="0.0" repeat="false" numRepeats="1" incrementParentId="roster" incrementField="points limit" incrementValue="1.0">
              <conditions>
                <condition parentId="roster" field="points limit" type="less than" value="0.0"/>
                <condition parentId="roster" field="points limit" type="greater than" value="0.0"/>
                <condition parentId="roster" field="points limit" type="instance of" value="0.0"/>
                <condition parentId="roster" field="points limit" type="not equal to" value="0.0"/>
                <condition parentId="roster" field="points limit" type="equal to" value="0.0"/>
                <condition parentId="roster" field="points limit" type="at most" value="0.0"/>
                <condition parentId="roster" field="points limit" type="at least" value="0.0"/>
                <condition parentId="roster" field="total selections" type="equal to" value="0.0"/>
                <condition parentId="d99ab4b1-4076-e79a-c3f3-287fcd813394" field="percent" type="equal to" value="0.0"/>
                <condition parentId="d99ab4b1-4076-e79a-c3f3-287fcd813394" field="points" type="equal to" value="0.0"/>
                <condition parentId="d99ab4b1-4076-e79a-c3f3-287fcd813394" field="selections" type="equal to" value="0.0"/>
              </conditions>
              <conditionGroups/>
            </modifier>
            <modifier type="set" field="minSelections" value="0.0" repeat="false" numRepeats="1" incrementParentId="roster" incrementField="points limit" incrementValue="1.0">
              <conditions/>
              <conditionGroups>
                <conditionGroup type="and">
                  <conditions>
                    <condition parentId="roster" field="points limit" type="equal to" value="0.0"/>
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
        </category>
      </categories>
      <forceTypes>
        <forceType id="cee5ca6c-940f-ec10-0584-39346e83c8f6" name="Child Force Type" minSelections="1" maxSelections="-1" minPoints="1.0" maxPoints="-1.0" minPercentage="1" maxPercentage="-1" countTowardsParentMinSelections="true" countTowardsParentMaxSelections="true" countTowardsParentMinPoints="true" countTowardsParentMaxPoints="true" countTowardsParentMinPercentage="true" countTowardsParentMaxPercentage="true">
          <categories>
            <category id="09bf771c-870a-a78e-7acc-5914b3e9ba1c" name="ChildCategory" minSelections="0" maxSelections="-1" minPoints="0.0" maxPoints="-1.0" minPercentage="0" maxPercentage="-1" countTowardsParentMinSelections="true" countTowardsParentMaxSelections="true" countTowardsParentMinPoints="false" countTowardsParentMaxPoints="false" countTowardsParentMinPercentage="false" countTowardsParentMaxPercentage="false">
              <modifiers/>
            </category>
          </categories>
          <forceTypes/>
        </forceType>
      </forceTypes>
    </forceType>
  </forceTypes>
  <profileTypes>
    <profileType id="7e6737a8-902d-b4b2-4cf7-55d4dfc723ba" name="Test Profile Type">
      <characteristics>
        <characteristic id="cc05a167-9e9a-6ae2-01da-633ea6aedde2" name="New Characteristic"/>
      </characteristics>
    </profileType>
  </profileTypes>
</gameSystem>