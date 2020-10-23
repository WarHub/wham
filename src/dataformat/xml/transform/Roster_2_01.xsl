<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" 
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
                xmlns:bsr="http://www.battlescribe.net/schema/rosterSchema" 
                xmlns="http://www.battlescribe.net/schema/rosterSchema"
                exclude-result-prefixes="bsr">

    <xsl:output method="xml" indent="yes"/>

    
    <!-- Roster -->
    <xsl:template match="/bsr:roster">
        <roster>
            <!-- Attributes -->
            <xsl:attribute name="battleScribeVersion">2.01</xsl:attribute>
            <xsl:apply-templates select="@*[name(.) != 'battleScribeVersion']"/>
            
            
            <!-- Nodes -->
            <xsl:apply-templates select="node()"/>
        </roster>
    </xsl:template>
    
    
    <!-- Force -->
    <xsl:template match="bsr:force">
        <force>
            <!-- Attributes -->
            <xsl:apply-templates select="@*"/>
            
            
            <!-- Nodes -->
            <selections>
            	<xsl:apply-templates select="bsr:categories/bsr:category/bsr:selections/*"/>
            </selections>
            
            <xsl:apply-templates select="node()[name(.) != 'categories']"/>
        </force>
    </xsl:template>
    
    
    <xsl:template match="* | bsr:*">
        <xsl:element name="{local-name(.)}">
            <xsl:apply-templates select="node() | @*"/>
        </xsl:element>
    </xsl:template>

    <xsl:template match="@*">
        <xsl:copy/>
    </xsl:template>
    
</xsl:stylesheet>
